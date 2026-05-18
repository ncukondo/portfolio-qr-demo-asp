namespace DoctorPortfolioSite.Application.CourseCompletions;

public interface ICourseCompletionService
{
    Task<RegisterCompletionResult> RegisterAsync(string userId, int courseId, DateTimeOffset? completedAt = null, CancellationToken cancellationToken = default);
}

public enum RegisterCompletionResult
{
    Created,
    AlreadyExists,
    CourseNotFound,
}
