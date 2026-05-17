using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Domain;

public class QrTokenTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_Sets_Properties_And_Audit()
    {
        var token = QrToken.Create(courseId: 100, token: "abc", expiresAt: Anchor.AddMinutes(5), now: Anchor);

        token.CourseId.Should().Be(100);
        token.Token.Should().Be("abc");
        token.ExpiresAt.Should().Be(Anchor.AddMinutes(5));
        token.IsUsed.Should().BeFalse();
        token.CreatedAt.Should().Be(Anchor);
    }

    [Fact]
    public void Create_With_Already_Past_ExpiresAt_Throws()
    {
        var act = () => QrToken.Create(100, "abc", Anchor, now: Anchor);
        act.Should().Throw<ArgumentException>().WithMessage("*expiresAt*");
    }

    [Fact]
    public void IsExpired_Returns_False_Before_Expiration()
    {
        var token = QrToken.Create(100, "abc", Anchor.AddMinutes(5), now: Anchor);

        token.IsExpired(Anchor.AddMinutes(4)).Should().BeFalse();
    }

    [Fact]
    public void IsExpired_Returns_True_At_Or_After_Expiration()
    {
        var token = QrToken.Create(100, "abc", Anchor.AddMinutes(5), now: Anchor);

        token.IsExpired(Anchor.AddMinutes(5)).Should().BeTrue();
        token.IsExpired(Anchor.AddMinutes(6)).Should().BeTrue();
    }

    [Fact]
    public void MarkUsed_Sets_IsUsed_True()
    {
        var token = QrToken.Create(100, "abc", Anchor.AddMinutes(5), now: Anchor);

        token.MarkUsed();

        token.IsUsed.Should().BeTrue();
    }

    [Fact]
    public void MarkUsed_When_Already_Used_Throws()
    {
        var token = QrToken.Create(100, "abc", Anchor.AddMinutes(5), now: Anchor);
        token.MarkUsed();

        var act = () => token.MarkUsed();
        act.Should().Throw<InvalidOperationException>();
    }
}
