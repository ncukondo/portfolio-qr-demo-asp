namespace DoctorPortfolioSite.Domain.Models;

public class Portfolio
{
    public int Id { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string LearningGoals { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private Portfolio() { }

    public static Portfolio Create(
        string userId,
        string title,
        string description,
        string learningGoals,
        DateTimeOffset? now = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("userId must not be empty.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("title must not be empty.", nameof(title));

        var stamp = now ?? DateTimeOffset.UtcNow;
        return new Portfolio
        {
            UserId = userId,
            Title = title,
            Description = description ?? string.Empty,
            LearningGoals = learningGoals ?? string.Empty,
            CreatedAt = stamp,
            UpdatedAt = stamp,
        };
    }

    public void UpdateGoals(string newGoals, DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(newGoals);
        LearningGoals = newGoals;
        UpdatedAt = now ?? DateTimeOffset.UtcNow;
    }
}
