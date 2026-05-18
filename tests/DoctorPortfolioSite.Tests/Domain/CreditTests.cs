using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Domain;

public class CreditTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_With_Valid_Args_Sets_Properties_And_Audit()
    {
        var credit = Credit.Create(
            code: "IT001",
            label: "Internal Medicine basics",
            category: "Internal",
            description: "Foundational internal medicine topics",
            now: Anchor);

        credit.Code.Should().Be("IT001");
        credit.Label.Should().Be("Internal Medicine basics");
        credit.Category.Should().Be("Internal");
        credit.Description.Should().Be("Foundational internal medicine topics");
        credit.CreatedAt.Should().Be(Anchor);
        credit.UpdatedAt.Should().Be(Anchor);
    }

    [Fact]
    public void Create_Allows_Blank_Category_And_Description()
    {
        var credit = Credit.Create("IT001", "Label", category: "", description: "", now: Anchor);

        credit.Category.Should().Be(string.Empty);
        credit.Description.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_With_Blank_Code_Throws(string blank)
    {
        var act = () => Credit.Create(blank, "Label", "Cat", "Desc");
        act.Should().Throw<ArgumentException>().WithMessage("*code*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_With_Blank_Label_Throws(string blank)
    {
        var act = () => Credit.Create("IT001", blank, "Cat", "Desc");
        act.Should().Throw<ArgumentException>().WithMessage("*label*");
    }
}
