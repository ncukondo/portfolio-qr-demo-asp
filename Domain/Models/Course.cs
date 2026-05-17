namespace DoctorPortfolioSite.Domain.Models;

public class Course
{
    public int Id { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public int Credits { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public string Venue { get; private set; } = string.Empty;
    public int MaxParticipants { get; private set; }
    public string QrCodeSecret { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Course() { }

    public static Course Create(
        string title,
        string description,
        int credits,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        string venue,
        int maxParticipants,
        string qrCodeSecret,
        DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("title must not be empty.", nameof(title));
        if (credits <= 0)
            throw new ArgumentOutOfRangeException(nameof(credits), "credits must be positive.");
        if (maxParticipants <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxParticipants), "maxParticipants must be positive.");
        if (endDate < startDate)
            throw new ArgumentException("endDate must not be earlier than startDate.", nameof(endDate));

        var stamp = now ?? DateTimeOffset.UtcNow;
        return new Course
        {
            Title = title,
            Description = description ?? string.Empty,
            Credits = credits,
            StartDate = startDate,
            EndDate = endDate,
            Venue = venue ?? string.Empty,
            MaxParticipants = maxParticipants,
            QrCodeSecret = qrCodeSecret ?? string.Empty,
            CreatedAt = stamp,
            UpdatedAt = stamp,
        };
    }
}
