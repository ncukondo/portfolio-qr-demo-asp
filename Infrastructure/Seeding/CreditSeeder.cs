using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Infrastructure.Seeding;

public class CreditSeeder
{
    public static readonly IReadOnlyList<CreditSeed> DefaultCredits = new[]
    {
        new CreditSeed("IT001", "内科一般", "Internal", "内科系の一般的な学習単位"),
        new CreditSeed("IT002", "症例検討", "Internal", "症例検討に基づく学習単位"),
        new CreditSeed("BZ001", "医療経営", "Business", "クリニック運営・経営に関する学習単位"),
        new CreditSeed("BZ002", "コミュニケーション", "Business", "患者・スタッフコミュニケーションに関する学習単位"),
        new CreditSeed("LG001", "医療法規", "Legal", "医療関連法規・規制に関する学習単位"),
        new CreditSeed("LG002", "医療倫理", "Legal", "医療倫理に関する学習単位"),
        new CreditSeed("SK001", "プライマリケア", "Skill", "プライマリケアに関する実践スキル"),
        new CreditSeed("SK002", "専門技術", "Skill", "専門領域の技術習得に関する学習単位"),
    };

    private readonly ApplicationDbContext _db;

    public CreditSeeder(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var existingCodes = await _db.Credits.Select(c => c.Code).ToListAsync(cancellationToken);
        var existing = new HashSet<string>(existingCodes, StringComparer.Ordinal);

        foreach (var seed in DefaultCredits)
        {
            if (existing.Contains(seed.Code))
                continue;

            _db.Credits.Add(Credit.Create(seed.Code, seed.Label, seed.Category, seed.Description));
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public sealed record CreditSeed(string Code, string Label, string Category, string Description);
}
