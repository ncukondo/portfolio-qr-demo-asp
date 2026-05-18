using DoctorPortfolioSite.Application.Courses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Tests.Application.Courses;

public class CourseServiceCreateTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 6, 1, 4, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateAsync_Persists_Course_With_Resolved_Credit_Codes()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();

        var id = await host.Service.CreateAsync(new CreateCourseInput(
            ClassName: "総合内科",
            Description: "desc",
            Organizer: "Org",
            EventDateTime: Anchor,
            DurationMinutes: 90,
            Credits: new[]
            {
                new CreditAssignment("IT001", 1.0m),
                new CreditAssignment("SK001", 1.5m),
            }));

        id.Should().BeGreaterThan(0);

        var course = await host.Db.Courses.Include(c => c.Credits).SingleAsync();
        course.ClassName.Should().Be("総合内科");
        course.Credits.Should().HaveCount(2);
        course.Credits.Sum(cc => cc.Amount).Should().Be(2.5m);
    }

    [Fact]
    public async Task CreateAsync_Silently_Ignores_Unknown_Credit_Codes()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();

        await host.Service.CreateAsync(new CreateCourseInput(
            ClassName: "Course",
            Description: "desc",
            Organizer: "Org",
            EventDateTime: Anchor,
            DurationMinutes: 60,
            Credits: new[]
            {
                new CreditAssignment("IT001", 1.0m),
                new CreditAssignment("XX999", 1.0m), // unknown
            }));

        var credits = await host.Db.ClassCredits.ToListAsync();
        credits.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateAsync_With_Blank_ClassName_Throws_And_Persists_Nothing()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();

        var act = async () => await host.Service.CreateAsync(new CreateCourseInput(
            ClassName: "",
            Description: "d",
            Organizer: "Org",
            EventDateTime: Anchor,
            DurationMinutes: 60,
            Credits: new[] { new CreditAssignment("IT001", 1.0m) }));

        await act.Should().ThrowAsync<ArgumentException>();

        (await host.Db.Courses.CountAsync()).Should().Be(0);
        (await host.Db.ClassCredits.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_Deduplicates_Repeated_Credit_Codes()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();

        await host.Service.CreateAsync(new CreateCourseInput(
            ClassName: "Course",
            Description: "d",
            Organizer: "Org",
            EventDateTime: Anchor,
            DurationMinutes: 60,
            Credits: new[]
            {
                new CreditAssignment("IT001", 1.0m),
                new CreditAssignment("IT001", 0.5m), // duplicate code — last one wins
            }));

        var classCredits = await host.Db.ClassCredits.ToListAsync();
        classCredits.Should().HaveCount(1);
        classCredits[0].Amount.Should().Be(0.5m);
    }
}
