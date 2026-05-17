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
            title: "Cardiology CME",
            description: "Continuing medical education on cardiology.",
            credits: 3,
            startDate: Anchor,
            endDate: Anchor.AddHours(2),
            venue: "Tokyo Hall",
            maxParticipants: 50,
            qrCodeSecret: "secret-xyz",
            now: Anchor);

        course.Title.Should().Be("Cardiology CME");
        course.Credits.Should().Be(3);
        course.MaxParticipants.Should().Be(50);
        course.QrCodeSecret.Should().Be("secret-xyz");
        course.CreatedAt.Should().Be(Anchor);
        course.UpdatedAt.Should().Be(Anchor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_With_Blank_Title_Throws(string blank)
    {
        var act = () => Course.Create(blank, "desc", 1, Anchor, Anchor.AddHours(1), "v", 10, "s");
        act.Should().Throw<ArgumentException>().WithMessage("*title*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_With_Non_Positive_Credits_Throws(int credits)
    {
        var act = () => Course.Create("t", "d", credits, Anchor, Anchor.AddHours(1), "v", 10, "s");
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*credits*");
    }

    [Fact]
    public void Create_With_Non_Positive_MaxParticipants_Throws()
    {
        var act = () => Course.Create("t", "d", 1, Anchor, Anchor.AddHours(1), "v", 0, "s");
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*maxParticipants*");
    }

    [Fact]
    public void Create_With_End_Before_Start_Throws()
    {
        var act = () => Course.Create("t", "d", 1, Anchor, Anchor.AddMinutes(-1), "v", 10, "s");
        act.Should().Throw<ArgumentException>().WithMessage("*endDate*");
    }
}
