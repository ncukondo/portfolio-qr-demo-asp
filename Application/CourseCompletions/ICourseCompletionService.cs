namespace DoctorPortfolioSite.Application.CourseCompletions;

public interface ICourseCompletionService
{
    Task<RegisterCompletionResult> RegisterAsync(string userId, int courseId, DateTimeOffset? completedAt = null, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserCompletionRow>> GetUserCompletionsAsync(string userId, CancellationToken cancellationToken = default);
}

public enum RegisterCompletionResult
{
    Created,
    AlreadyExists,
    CourseNotFound,
}

public sealed record UserCompletionRow(
    int CourseId,
    string ClassName,
    string Organizer,
    DateTimeOffset EventDateTime,
    DateTimeOffset CompletedAt);
