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
        Guard.AgainstNonPositive(courseId);
        Guard.AgainstBlank(token);

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

    public bool IsExpired(DateTimeOffset moment) => ExpiresAt <= moment;

    public void MarkUsed()
    {
        if (IsUsed)
            throw new InvalidOperationException("Token has already been used.");
        IsUsed = true;
    }
}
