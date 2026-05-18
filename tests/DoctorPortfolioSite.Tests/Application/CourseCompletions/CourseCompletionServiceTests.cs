using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Application.CourseCompletions;
using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Tests.Application.CourseCompletions;

public class CourseCompletionServiceTests
{
    private static async Task<(ApplicationDbContext Db, ICourseCompletionService Svc, int CourseId)> SetupAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new ApplicationDbContext(options);

        await new CreditSeeder(db).SeedAsync();
        var courseService = new CourseService(db);
        var id = await courseService.CreateAsync(new CreateCourseInput(
            "Comp-Course", "d", "Org",
            new DateTimeOffset(2026, 6, 1, 4, 0, 0, TimeSpan.Zero),
            60,
            new[] { new CreditAssignment("IT001", 1.0m) }));

        return (db, new CourseCompletionService(db), id);
    }

    [Fact]
    public async Task RegisterAsync_Returns_Created_For_First_Registration()
    {
        var (db, svc, id) = await SetupAsync();
        await using var _ = db;

        var result = await svc.RegisterAsync("user-1", id);

        result.Should().Be(RegisterCompletionResult.Created);
        (await db.CourseCompletions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RegisterAsync_Returns_AlreadyExists_On_Second_Call_Same_User_Course()
    {
        var (db, svc, id) = await SetupAsync();
        await using var _ = db;

        await svc.RegisterAsync("user-1", id);
        var second = await svc.RegisterAsync("user-1", id);

        second.Should().Be(RegisterCompletionResult.AlreadyExists);
        (await db.CourseCompletions.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task RegisterAsync_Allows_Different_User_Same_Course()
    {
        var (db, svc, id) = await SetupAsync();
        await using var _ = db;

        (await svc.RegisterAsync("user-1", id)).Should().Be(RegisterCompletionResult.Created);
        (await svc.RegisterAsync("user-2", id)).Should().Be(RegisterCompletionResult.Created);
        (await db.CourseCompletions.CountAsync()).Should().Be(2);
    }

    [Fact]
    public async Task RegisterAsync_Returns_CourseNotFound_When_Course_Missing()
    {
        var (db, svc, _) = await SetupAsync();
        await using var _0 = db;

        var result = await svc.RegisterAsync("user-1", courseId: 9999);
        result.Should().Be(RegisterCompletionResult.CourseNotFound);
        (await db.CourseCompletions.CountAsync()).Should().Be(0);
    }
}
