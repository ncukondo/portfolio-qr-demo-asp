namespace DoctorPortfolioSite.Domain.Models;

public class CourseCompletion
{
    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public int CourseId { get; private set; }
    public DateTimeOffset CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private CourseCompletion() { }

    public static CourseCompletion Create(
        string userId,
        int courseId,
        DateTimeOffset completedAt,
        DateTimeOffset? now = null)
    {
        Guard.AgainstBlank(userId);
        Guard.AgainstNonPositive(courseId);

        var stamp = now ?? DateTimeOffset.UtcNow;
        return new CourseCompletion
        {
            UserId = userId,
            CourseId = courseId,
            CompletedAt = completedAt,
            CreatedAt = stamp,
            UpdatedAt = stamp,
        };
    }
}
