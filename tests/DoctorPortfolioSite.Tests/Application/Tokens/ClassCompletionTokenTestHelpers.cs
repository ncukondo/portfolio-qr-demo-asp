using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace DoctorPortfolioSite.Tests.Application.Tokens;

internal static class ClassCompletionTokenTestHelpers
{
    public static string GenerateWithPurpose(string secret, string issuer, string purpose, int[] classIds)
        => GenerateInternal(secret, issuer, purpose, classIds, DateTimeOffset.UtcNow.AddHours(1));

    public static string GenerateExpired(string secret, string issuer, int[] classIds)
        => GenerateInternal(secret, issuer, "class_completion", classIds, DateTimeOffset.UtcNow.AddHours(-1));

    private static string GenerateInternal(string secret, string issuer, string purpose, int[] classIds, DateTimeOffset exp)
    {
        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            SecurityAlgorithms.HmacSha256);

        var now = DateTimeOffset.UtcNow;
        var iat = exp <= now ? exp.AddHours(-1) : now;

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: null,
            claims: new[]
            {
                new Claim("purpose", purpose),
                new Claim("class_ids", string.Join(",", classIds)),
            },
            notBefore: iat.UtcDateTime,
            expires: exp.UtcDateTime,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
