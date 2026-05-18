using DoctorPortfolioSite.Application.Courses;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Application.Courses;

public class CourseServiceGetByIdTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 6, 1, 4, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetByIdAsync_Returns_Course_With_Credits()
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

        var details = await host.Service.GetByIdAsync(id);

        details.Should().NotBeNull();
        details!.Id.Should().Be(id);
        details.ClassName.Should().Be("総合内科");
        details.EventDateTime.Should().Be(Anchor);
        details.Credits.Should().HaveCount(2);
        details.Credits.Single(c => c.Code == "IT001").Amount.Should().Be(1.0m);
        details.Credits.Single(c => c.Code == "SK001").Label.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Null_For_Missing_Id()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();

        var details = await host.Service.GetByIdAsync(9999);

        details.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_Returns_Empty_Credits_When_None_Linked()
    {
        await using var host = await CourseServiceTestHost.CreateAsync();
        var id = await host.Service.CreateAsync(new CreateCourseInput(
            "Course", "d", "Org", Anchor, 60, Array.Empty<CreditAssignment>()));

        var details = await host.Service.GetByIdAsync(id);

        details!.Credits.Should().BeEmpty();
    }
}
