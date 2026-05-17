using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Domain;

public class EnrollmentTests
{
    private static readonly DateTimeOffset EnrolledAt = new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_Yields_Enrolled_Status_With_Audit_Fields()
    {
        var enrollment = Enrollment.Create(userId: "user-1", courseId: 100, enrolledAt: EnrolledAt);

        enrollment.UserId.Should().Be("user-1");
        enrollment.CourseId.Should().Be(100);
        enrollment.EnrolledAt.Should().Be(EnrolledAt);
        enrollment.Status.Should().Be(EnrollmentStatus.Enrolled);
        enrollment.CompletedAt.Should().BeNull();
        enrollment.QrCodeScannedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_With_Blank_UserId_Throws(string blank)
    {
        var act = () => Enrollment.Create(blank, 100, EnrolledAt);
        act.Should().Throw<ArgumentException>().WithMessage("*userId*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_With_Non_Positive_CourseId_Throws(int courseId)
    {
        var act = () => Enrollment.Create("user-1", courseId, EnrolledAt);
        act.Should().Throw<ArgumentOutOfRangeException>().WithMessage("*courseId*");
    }

    [Fact]
    public void MarkCompleted_Sets_Status_CompletedAt_And_QrScannedAt()
    {
        var enrollment = Enrollment.Create("user-1", 100, EnrolledAt);
        var scannedAt = EnrolledAt.AddHours(2);

        enrollment.MarkCompleted(scannedAt);

        enrollment.Status.Should().Be(EnrollmentStatus.Completed);
        enrollment.CompletedAt.Should().Be(scannedAt);
        enrollment.QrCodeScannedAt.Should().Be(scannedAt);
    }

    [Fact]
    public void MarkCompleted_When_Already_Completed_Throws()
    {
        var enrollment = Enrollment.Create("user-1", 100, EnrolledAt);
        enrollment.MarkCompleted(EnrolledAt.AddHours(2));

        var act = () => enrollment.MarkCompleted(EnrolledAt.AddHours(3));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkCompleted_When_Cancelled_Throws()
    {
        var enrollment = Enrollment.Create("user-1", 100, EnrolledAt);
        enrollment.Cancel();

        var act = () => enrollment.MarkCompleted(EnrolledAt.AddHours(2));
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_Sets_Status_Cancelled()
    {
        var enrollment = Enrollment.Create("user-1", 100, EnrolledAt);

        enrollment.Cancel();

        enrollment.Status.Should().Be(EnrollmentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_When_Completed_Throws()
    {
        var enrollment = Enrollment.Create("user-1", 100, EnrolledAt);
        enrollment.MarkCompleted(EnrolledAt.AddHours(2));

        var act = () => enrollment.Cancel();
        act.Should().Throw<InvalidOperationException>();
    }
}
