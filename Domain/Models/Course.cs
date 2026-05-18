namespace DoctorPortfolioSite.Domain.Models;

public class Course
{
    public int Id { get; private set; }
    public string ClassName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Organizer { get; private set; } = string.Empty;
    public DateTimeOffset EventDateTime { get; private set; }
    public int DurationMinutes { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Course() { }

    public static Course Create(
        string className,
        string description,
        string organizer,
        DateTimeOffset eventDateTime,
        int durationMinutes,
        DateTimeOffset? now = null)
    {
        Guard.AgainstBlank(className);
        Guard.AgainstBlank(organizer);
        Guard.AgainstNonPositive(durationMinutes);

        var stamp = now ?? DateTimeOffset.UtcNow;
        return new Course
        {
            ClassName = className,
            Description = description ?? string.Empty,
            Organizer = organizer,
            EventDateTime = eventDateTime,
            DurationMinutes = durationMinutes,
            CreatedAt = stamp,
            UpdatedAt = stamp,
        };
    }
}
