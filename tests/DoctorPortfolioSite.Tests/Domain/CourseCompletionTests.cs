using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Domain;

public class CourseCompletionTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_With_Valid_Args_Sets_Properties_And_Audit()
    {
        var completion = CourseCompletion.Create(
            userId: "user-1",
            courseId: 100,
            completedAt: Anchor,
            now: Anchor);

        completion.UserId.Should().Be("user-1");
        completion.CourseId.Should().Be(100);
        completion.CompletedAt.Should().Be(Anchor);
        completion.CreatedAt.Should().Be(Anchor);
        completion.UpdatedAt.Should().Be(Anchor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_With_Blank_UserId_Throws(string blank)
    {
        var act = () => CourseCompletion.Create(blank, 100, Anchor);
        act.Should().Throw<ArgumentException>().WithMessage("*userId*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_With_Non_Positive_CourseId_Throws(int courseId)
    {
        var act = () => CourseCompletion.Create("user-1", courseId, Anchor);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*courseId*");
    }
}
