namespace DoctorPortfolioSite.Domain.Models;

public class Enrollment
{
    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public int CourseId { get; private set; }
    public DateTimeOffset EnrolledAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateTimeOffset? QrCodeScannedAt { get; private set; }

    private Enrollment() { }

    public static Enrollment Create(string userId, int courseId, DateTimeOffset enrolledAt)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId must not be empty.", nameof(userId));
        if (courseId <= 0)
            throw new ArgumentOutOfRangeException(nameof(courseId), "courseId must be positive.");

        return new Enrollment
        {
            UserId = userId,
            CourseId = courseId,
            EnrolledAt = enrolledAt,
            Status = EnrollmentStatus.Enrolled,
        };
    }

    public void MarkCompleted(DateTimeOffset scannedAt)
    {
        if (Status != EnrollmentStatus.Enrolled)
            throw new InvalidOperationException($"Cannot complete an enrollment in status {Status}.");

        Status = EnrollmentStatus.Completed;
        CompletedAt = scannedAt;
        QrCodeScannedAt = scannedAt;
    }

    public void Cancel()
    {
        if (Status == EnrollmentStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed enrollment.");

        Status = EnrollmentStatus.Cancelled;
    }
}
