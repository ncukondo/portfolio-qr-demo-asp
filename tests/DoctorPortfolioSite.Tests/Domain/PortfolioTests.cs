using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Domain;

public class PortfolioTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_With_Valid_Args_Sets_Properties_And_Audit()
    {
        var portfolio = Portfolio.Create(
            userId: "user-1",
            title: "Cardiology log",
            description: "My CME records",
            learningGoals: "Read 5 papers/month",
            now: Anchor);

        portfolio.UserId.Should().Be("user-1");
        portfolio.Title.Should().Be("Cardiology log");
        portfolio.LearningGoals.Should().Be("Read 5 papers/month");
        portfolio.CreatedAt.Should().Be(Anchor);
        portfolio.UpdatedAt.Should().Be(Anchor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Create_With_Blank_UserId_Throws(string blank)
    {
        var act = () => Portfolio.Create(blank, "t", "d", "g", Anchor);
        act.Should().Throw<ArgumentException>().WithMessage("*userId*");
    }

    [Fact]
    public void Create_With_Blank_Title_Throws()
    {
        var act = () => Portfolio.Create("user-1", "", "d", "g", Anchor);
        act.Should().Throw<ArgumentException>().WithMessage("*title*");
    }

    [Fact]
    public void UpdateGoals_Replaces_Goals_And_Bumps_UpdatedAt()
    {
        var portfolio = Portfolio.Create("user-1", "t", "d", "old goals", Anchor);
        var later = Anchor.AddDays(1);

        portfolio.UpdateGoals("new goals", later);

        portfolio.LearningGoals.Should().Be("new goals");
        portfolio.UpdatedAt.Should().Be(later);
        portfolio.CreatedAt.Should().Be(Anchor);
    }
}
