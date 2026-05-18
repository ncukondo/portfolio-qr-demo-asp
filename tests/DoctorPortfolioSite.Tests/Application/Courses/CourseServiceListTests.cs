using DoctorPortfolioSite.Application.Courses;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Application.Courses;

public class CourseServiceListTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 6, 1, 4, 0, 0, TimeSpan.Zero);

    private static async Task SeedThreeCoursesAsync(CourseServiceTestHost host)
    {
        await host.Service.CreateAsync(new CreateCourseInput(
            "B course", "d", "Acme",  Anchor.AddDays(2), 60,
            new[] { new CreditAssignment("IT001", 1.0m), new CreditAssignment("IT002", 0.5m) }));
        await host.Service.CreateAsync(new CreateCourseInput(
            "A course", "d", "Beta",  Anchor,             60,
            new[] { new CreditAssignment("BZ001", 2.0m) }));
        await host.Service.CreateAsync(new CreateCourseInput(
            "C course", "d", "Acme",  Anchor.AddDays(10), 60,
            Array.Empty<CreditAssignment>()));
    }

    [Fact]
    public async Task ListAsync_Returns_Courses_Sorted_By_EventDateTime_Asc()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();
        await SeedThreeCoursesAsync(host);

        var results = await host.Service.ListAsync(new CourseQuery());

        results.Select(r => r.ClassName).Should().ContainInOrder("A course", "B course", "C course");
    }

    [Fact]
    public async Task ListAsync_Filters_By_Organizer()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();
        await SeedThreeCoursesAsync(host);

        var results = await host.Service.ListAsync(new CourseQuery(Organizer: "Acme"));

        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.Organizer == "Acme");
    }

    [Fact]
    public async Task ListAsync_Filters_By_DateRange()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();
        await SeedThreeCoursesAsync(host);

        var results = await host.Service.ListAsync(new CourseQuery(
            From: Anchor.AddDays(1),
            To: Anchor.AddDays(5)));

        results.Should().HaveCount(1);
        results.Single().ClassName.Should().Be("B course");
    }

    [Fact]
    public async Task ListAsync_Aggregates_TotalCreditAmount()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();
        await SeedThreeCoursesAsync(host);

        var results = await host.Service.ListAsync(new CourseQuery());

        results.Single(r => r.ClassName == "B course").TotalCreditAmount.Should().Be(1.5m);
        results.Single(r => r.ClassName == "C course").TotalCreditAmount.Should().Be(0m);
    }
}
