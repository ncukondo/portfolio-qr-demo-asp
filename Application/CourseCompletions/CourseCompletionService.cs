using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Application.CourseCompletions;

public class CourseCompletionService : ICourseCompletionService
{
    private readonly ApplicationDbContext _db;

    public CourseCompletionService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<RegisterCompletionResult> RegisterAsync(
        string userId,
        int courseId,
        DateTimeOffset? completedAt = null,
        CancellationToken cancellationToken = default)
    {
        var courseExists = await _db.Courses.AnyAsync(c => c.Id == courseId, cancellationToken);
        if (!courseExists)
            return RegisterCompletionResult.CourseNotFound;

        var alreadyCompleted = await _db.CourseCompletions
            .AnyAsync(cc => cc.UserId == userId && cc.CourseId == courseId, cancellationToken);
        if (alreadyCompleted)
            return RegisterCompletionResult.AlreadyExists;

        var stamp = completedAt ?? DateTimeOffset.UtcNow;
        _db.CourseCompletions.Add(CourseCompletion.Create(userId, courseId, stamp));
        await _db.SaveChangesAsync(cancellationToken);
        return RegisterCompletionResult.Created;
    }

    public async Task<IReadOnlyList<UserCompletionRow>> GetUserCompletionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await (
            from cc in _db.CourseCompletions.AsNoTracking()
            join c in _db.Courses.AsNoTracking() on cc.CourseId equals c.Id
            where cc.UserId == userId
            orderby cc.CompletedAt descending
            select new UserCompletionRow(c.Id, c.ClassName, c.Organizer, c.EventDateTime, cc.CompletedAt)
        ).ToListAsync(cancellationToken);
    }
}
