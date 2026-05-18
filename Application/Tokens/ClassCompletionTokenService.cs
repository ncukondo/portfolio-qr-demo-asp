using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DoctorPortfolioSite.Application.Tokens;

public class ClassCompletionTokenService : ICompletionTokenService
{
    public const string PurposeValue = "class_completion";

    private const int MinExpirationHours = 1;
    private const int MaxExpirationHours = 8760; // 365 days

    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _handler = new();

    public ClassCompletionTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.Secret))
            throw new InvalidOperationException("Jwt:Secret is not configured.");
    }

    public string Generate(int[] classIds, int expirationHours = 24)
    {
        ArgumentNullException.ThrowIfNull(classIds);
        if (classIds.Length == 0)
            throw new ArgumentException("classIds must not be empty.", nameof(classIds));
        if (expirationHours < MinExpirationHours || expirationHours > MaxExpirationHours)
            throw new ArgumentOutOfRangeException(nameof(expirationHours),
                $"expirationHours must be between {MinExpirationHours} and {MaxExpirationHours}.");

        var now = DateTimeOffset.UtcNow;
        var exp = now.AddHours(expirationHours);

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("purpose", PurposeValue),
            new("class_ids", string.Join(",", classIds)),
            new("created_at", now.ToString("O")),
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: null,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: exp.UtcDateTime,
            signingCredentials: creds);

        return _handler.WriteToken(token);
    }

    public CompletionTokenPayload? TryDecode(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret)),
            ClockSkew = TimeSpan.Zero,
        };

        try
        {
            var principal = _handler.ValidateToken(token, parameters, out var validated);
            var jwt = (JwtSecurityToken)validated;

            var purpose = principal.FindFirst("purpose")?.Value;
            if (!string.Equals(purpose, PurposeValue, StringComparison.Ordinal))
                return null;

            var classIdsRaw = principal.FindFirst("class_ids")?.Value;
            if (string.IsNullOrWhiteSpace(classIdsRaw))
                return null;

            var classIds = classIdsRaw
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .ToArray();

            if (classIds.Length == 0)
                return null;

            return new CompletionTokenPayload(
                purpose!,
                classIds,
                new DateTimeOffset(jwt.ValidFrom, TimeSpan.Zero),
                new DateTimeOffset(jwt.ValidTo, TimeSpan.Zero));
        }
        catch
        {
            return null;
        }
    }

    public string BuildCompletionUrl(int[] classIds, string baseUrl, int expirationHours = 24)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);

        var token = Generate(classIds, expirationHours);
        var trimmed = baseUrl.TrimEnd('/');
        return $"{trimmed}/Complete?token={Uri.EscapeDataString(token)}";
    }
}
