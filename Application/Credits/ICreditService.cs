namespace DoctorPortfolioSite.Application.Credits;

public interface ICreditService
{
    Task<IReadOnlyList<CreditOption>> ListAsync(CancellationToken cancellationToken = default);
}

public sealed record CreditOption(string Code, string Label, string Category);
