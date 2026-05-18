using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Application.Courses;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _db;

    public CourseService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateAsync(CreateCourseInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Build Course first; entity-level Guard validations throw before any DB write.
        var course = Course.Create(
            input.ClassName,
            input.Description,
            input.Organizer,
            input.EventDateTime,
            input.DurationMinutes);

        var resolved = await ResolveCreditAssignmentsAsync(input.Credits, cancellationToken);

        _db.Courses.Add(course);
        await _db.SaveChangesAsync(cancellationToken);

        foreach (var (creditId, amount) in resolved)
        {
            _db.ClassCredits.Add(ClassCredit.Create(course.Id, creditId, amount));
        }

        if (resolved.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return course.Id;
    }

    public async Task<CourseDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (course is null)
            return null;

        var credits = await (
            from cc in _db.ClassCredits.AsNoTracking()
            join c in _db.Credits.AsNoTracking() on cc.CreditId equals c.Id
            where cc.CourseId == id
            orderby c.Code
            select new CourseCreditDto(c.Code, c.Label, cc.Amount)
        ).ToListAsync(cancellationToken);

        return new CourseDetailsDto(
            course.Id,
            course.ClassName,
            course.Description,
            course.Organizer,
            course.EventDateTime,
            course.DurationMinutes,
            course.CreatedAt,
            course.UpdatedAt,
            credits);
    }

    public Task<IReadOnlyList<CourseDto>> ListAsync(CourseQuery query, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> UpdateAsync(int id, UpdateCourseInput input, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    // Resolves credit codes → (id, amount). Unknown codes silently dropped (PHP parity).
    // Duplicate codes are collapsed; the last assignment wins.
    private async Task<IReadOnlyList<(int CreditId, decimal Amount)>> ResolveCreditAssignmentsAsync(
        IReadOnlyList<CreditAssignment> assignments,
        CancellationToken cancellationToken)
    {
        if (assignments.Count == 0)
            return Array.Empty<(int, decimal)>();

        var byCode = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var a in assignments)
            byCode[a.Code] = a.Amount;

        var codes = byCode.Keys.ToList();
        var creditIdByCode = await _db.Credits
            .Where(c => codes.Contains(c.Code))
            .Select(c => new { c.Code, c.Id })
            .ToDictionaryAsync(x => x.Code, x => x.Id, StringComparer.Ordinal, cancellationToken);

        var result = new List<(int CreditId, decimal Amount)>(byCode.Count);
        foreach (var (code, amount) in byCode)
        {
            if (creditIdByCode.TryGetValue(code, out var id))
                result.Add((id, amount));
        }
        return result;
    }
}
