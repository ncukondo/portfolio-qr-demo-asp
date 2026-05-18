namespace DoctorPortfolioSite.Infrastructure.Seeding;

public class SeedingOptions
{
    public const string SectionName = "Seeding";

    public bool LoadSampleData { get; set; }

    public string SampleUserPassword { get; set; } = "Password123!";
}
