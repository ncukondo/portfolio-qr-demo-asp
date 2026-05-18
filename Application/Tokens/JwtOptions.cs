namespace DoctorPortfolioSite.Application.Tokens;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;

    public string Issuer { get; set; } = "portfolio-system";
}
