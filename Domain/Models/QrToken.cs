namespace DoctorPortfolioSite.Domain.Models;

public class QrToken
{
    public int Id { get; private set; }
    public int CourseId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public bool IsUsed { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private QrToken() { }

    public static QrToken Create(int courseId, string token, DateTimeOffset expiresAt, DateTimeOffset? now = null)
    {
        if (courseId <= 0)
            throw new ArgumentOutOfRangeException(nameof(courseId), "courseId must be positive.");
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("token must not be empty.", nameof(token));

        var stamp = now ?? DateTimeOffset.UtcNow;
        if (expiresAt <= stamp)
            throw new ArgumentException("expiresAt must be in the future.", nameof(expiresAt));

        return new QrToken
        {
            CourseId = courseId,
            Token = token,
            ExpiresAt = expiresAt,
            IsUsed = false,
            CreatedAt = stamp,
        };
    }

    public void MarkUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Token has already been used.");
        IsUsed = true;
    }
}
