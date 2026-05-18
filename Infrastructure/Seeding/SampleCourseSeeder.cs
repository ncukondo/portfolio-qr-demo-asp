using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Infrastructure.Seeding;

public class SampleCourseSeeder
{
    private static readonly DateTimeOffset BaseDate = new(2026, 6, 1, 13, 0, 0, TimeSpan.FromHours(9));

    public static readonly IReadOnlyList<CourseSeed> DefaultCourses = new[]
    {
        new CourseSeed(
            ClassName: "総合内科基礎セミナー",
            Description: "総合内科の基本的なトピックを扱う入門セミナー。",
            Organizer: "東京内科学会",
            EventDateTime: BaseDate,
            DurationMinutes: 120,
            Credits: new[] { new CourseCreditSeed("IT001", 1.0m), new CourseCreditSeed("IT002", 0.5m) }),
        new CourseSeed(
            ClassName: "プライマリケア最新情報",
            Description: "プライマリケアの最新エビデンスと実践。",
            Organizer: "日本プライマリケア学会",
            EventDateTime: BaseDate.AddDays(7),
            DurationMinutes: 90,
            Credits: new[] { new CourseCreditSeed("IT001", 1.0m), new CourseCreditSeed("SK001", 1.0m) }),
        new CourseSeed(
            ClassName: "医療マネジメント講座",
            Description: "クリニック運営・人事・財務の実務講座。",
            Organizer: "メディカルマネジメント協会",
            EventDateTime: BaseDate.AddDays(14),
            DurationMinutes: 180,
            Credits: new[] { new CourseCreditSeed("BZ001", 1.5m), new CourseCreditSeed("BZ002", 0.5m) }),
        new CourseSeed(
            ClassName: "医療法規セミナー",
            Description: "改正点を中心とした医療関連法規アップデート。",
            Organizer: "医療法務研究会",
            EventDateTime: BaseDate.AddDays(21),
            DurationMinutes: 60,
            Credits: new[] { new CourseCreditSeed("LG001", 1.0m), new CourseCreditSeed("LG002", 0.5m) }),
        new CourseSeed(
            ClassName: "症例検討会:循環器",
            Description: "循環器内科の症例検討と専門技術アップデート。",
            Organizer: "心血管医学会",
            EventDateTime: BaseDate.AddDays(28),
            DurationMinutes: 150,
            Credits: new[] { new CourseCreditSeed("IT002", 1.0m), new CourseCreditSeed("SK002", 1.5m) }),
    };

    private readonly ApplicationDbContext _db;

    public SampleCourseSeeder(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var creditIdByCode = await _db.Credits.ToDictionaryAsync(c => c.Code, c => c.Id, StringComparer.Ordinal, cancellationToken);

        foreach (var seed in DefaultCourses)
        {
            foreach (var link in seed.Credits)
            {
                if (!creditIdByCode.ContainsKey(link.Code))
                    throw new InvalidOperationException(
                        $"Cannot seed sample course '{seed.ClassName}': credit code '{link.Code}' is missing. Run CreditSeeder first.");
            }

            var course = await _db.Courses.FirstOrDefaultAsync(c => c.ClassName == seed.ClassName, cancellationToken);
            if (course is null)
            {
                course = Course.Create(seed.ClassName, seed.Description, seed.Organizer, seed.EventDateTime, seed.DurationMinutes);
                _db.Courses.Add(course);
                await _db.SaveChangesAsync(cancellationToken);
            }

            var existingCreditIds = await _db.ClassCredits
                .Where(cc => cc.CourseId == course.Id)
                .Select(cc => cc.CreditId)
                .ToListAsync(cancellationToken);
            var existing = new HashSet<int>(existingCreditIds);

            foreach (var link in seed.Credits)
            {
                var creditId = creditIdByCode[link.Code];
                if (existing.Contains(creditId))
                    continue;

                _db.ClassCredits.Add(ClassCredit.Create(course.Id, creditId, link.Amount));
            }

            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public sealed record CourseSeed(
        string ClassName,
        string Description,
        string Organizer,
        DateTimeOffset EventDateTime,
        int DurationMinutes,
        IReadOnlyList<CourseCreditSeed> Credits);

    public sealed record CourseCreditSeed(string Code, decimal Amount);
}
