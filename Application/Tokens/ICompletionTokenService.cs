namespace DoctorPortfolioSite.Application.Tokens;

public interface ICompletionTokenService
{
    string Generate(int[] classIds, int expirationHours = 24);

    CompletionTokenPayload? TryDecode(string token);

    string BuildCompletionUrl(int[] classIds, string baseUrl, int expirationHours = 24);
}

public sealed record CompletionTokenPayload(
    string Purpose,
    IReadOnlyList<int> ClassIds,
    DateTimeOffset IssuedAt,
    DateTimeOffset ExpiresAt);
