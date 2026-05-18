using DoctorPortfolioSite.Application.Courses;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Tests.Application.Courses;

public class CourseServiceUpdateDeleteTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 6, 1, 4, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task UpdateAsync_Replaces_Fields_And_Credit_Links()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();
        var id = await host.Service.CreateAsync(new CreateCourseInput(
            "Before", "d", "Org", Anchor, 60,
            new[] { new CreditAssignment("IT001", 1.0m) }));

        var ok = await host.Service.UpdateAsync(id, new UpdateCourseInput(
            "After", "d2", "Org2", Anchor.AddDays(1), 90,
            new[] { new CreditAssignment("BZ001", 2.0m), new CreditAssignment("SK001", 0.5m) }));

        ok.Should().BeTrue();
        var details = await host.Service.GetByIdAsync(id);
        details!.ClassName.Should().Be("After");
        details.Organizer.Should().Be("Org2");
        details.DurationMinutes.Should().Be(90);
        details.Credits.Select(c => c.Code).Should().BeEquivalentTo(new[] { "BZ001", "SK001" });
    }

    [Fact]
    public async Task UpdateAsync_Returns_False_When_Course_Missing()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();

        var ok = await host.Service.UpdateAsync(9999, new UpdateCourseInput(
            "x", "d", "Org", Anchor, 60, Array.Empty<CreditAssignment>()));

        ok.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_With_Blank_ClassName_Throws_And_Persists_Nothing()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();
        var id = await host.Service.CreateAsync(new CreateCourseInput(
            "Before", "d", "Org", Anchor, 60, new[] { new CreditAssignment("IT001", 1.0m) }));

        var act = async () => await host.Service.UpdateAsync(id, new UpdateCourseInput(
            "", "d", "Org", Anchor, 60, Array.Empty<CreditAssignment>()));
        await act.Should().ThrowAsync<ArgumentException>();

        var details = await host.Service.GetByIdAsync(id);
        details!.ClassName.Should().Be("Before");
        details.Credits.Should().HaveCount(1);
    }

    [Fact]
    public async Task DeleteAsync_Removes_Course_And_Its_ClassCredits()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();
        var id = await host.Service.CreateAsync(new CreateCourseInput(
            "X", "d", "Org", Anchor, 60,
            new[] { new CreditAssignment("IT001", 1.0m), new CreditAssignment("BZ001", 1.0m) }));

        var ok = await host.Service.DeleteAsync(id);

        ok.Should().BeTrue();
        (await host.Db.Courses.CountAsync()).Should().Be(0);
        (await host.Db.ClassCredits.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_Returns_False_For_Missing_Id()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();

        (await host.Service.DeleteAsync(9999)).Should().BeFalse();
    }
}
