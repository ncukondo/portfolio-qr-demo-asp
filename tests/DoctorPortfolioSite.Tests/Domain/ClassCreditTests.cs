using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Domain;

public class ClassCreditTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_With_Valid_Args_Sets_Properties()
    {
        var classCredit = ClassCredit.Create(courseId: 10, creditId: 5, amount: 1.5m, now: Anchor);

        classCredit.CourseId.Should().Be(10);
        classCredit.CreditId.Should().Be(5);
        classCredit.Amount.Should().Be(1.5m);
        classCredit.CreatedAt.Should().Be(Anchor);
        classCredit.UpdatedAt.Should().Be(Anchor);
    }

    [Fact]
    public void Create_Defaults_Amount_To_One_When_Omitted()
    {
        var classCredit = ClassCredit.Create(courseId: 10, creditId: 5, now: Anchor);

        classCredit.Amount.Should().Be(1.0m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_With_Non_Positive_CourseId_Throws(int courseId)
    {
        var act = () => ClassCredit.Create(courseId, 5, 1.0m);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*courseId*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_With_Non_Positive_CreditId_Throws(int creditId)
    {
        var act = () => ClassCredit.Create(10, creditId, 1.0m);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*creditId*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.5)]
    public void Create_With_Non_Positive_Amount_Throws(decimal amount)
    {
        var act = () => ClassCredit.Create(10, 5, amount);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*amount*");
    }
}
