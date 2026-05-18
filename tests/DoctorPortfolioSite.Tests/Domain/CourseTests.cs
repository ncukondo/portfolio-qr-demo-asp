using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Domain;

public class CourseTests
{
    private static DateTimeOffset Anchor => new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_With_Valid_Args_Sets_Properties()
    {
        var course = Course.Create(
            className: "Cardiology CME",
            description: "Continuing medical education on cardiology.",
            organizer: "Tokyo Heart Society",
            eventDateTime: Anchor,
            durationMinutes: 120,
            now: Anchor);

        course.ClassName.Should().Be("Cardiology CME");
        course.Description.Should().Be("Continuing medical education on cardiology.");
        course.Organizer.Should().Be("Tokyo Heart Society");
        course.EventDateTime.Should().Be(Anchor);
        course.DurationMinutes.Should().Be(120);
        course.CreatedAt.Should().Be(Anchor);
        course.UpdatedAt.Should().Be(Anchor);
        course.Credits.Should().BeEmpty();
    }

    [Fact]
    public void Create_Allows_Blank_Description()
    {
        var course = Course.Create("CME", description: "", organizer: "Org", eventDateTime: Anchor, durationMinutes: 60, now: Anchor);

        course.Description.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_With_Blank_ClassName_Throws(string blank)
    {
        var act = () => Course.Create(blank, "desc", "Org", Anchor, 60);
        act.Should().Throw<ArgumentException>().WithMessage("*className*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_With_Blank_Organizer_Throws(string blank)
    {
        var act = () => Course.Create("CME", "desc", blank, Anchor, 60);
        act.Should().Throw<ArgumentException>().WithMessage("*organizer*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_With_Non_Positive_DurationMinutes_Throws(int duration)
    {
        var act = () => Course.Create("CME", "desc", "Org", Anchor, duration);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*durationMinutes*");
    }
}
