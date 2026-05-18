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

    public async Task<IReadOnlyList<CourseDto>> ListAsync(CourseQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var courses = _db.Courses.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Organizer))
            courses = courses.Where(c => c.Organizer == query.Organizer);
        if (query.From.HasValue)
            courses = courses.Where(c => c.EventDateTime >= query.From.Value);
        if (query.To.HasValue)
            courses = courses.Where(c => c.EventDateTime <= query.To.Value);

        var rows = await (
            from c in courses
            orderby c.EventDateTime
            select new
            {
                c.Id,
                c.ClassName,
                c.Organizer,
                c.EventDateTime,
                c.DurationMinutes,
                TotalCreditAmount = _db.ClassCredits
                    .Where(cc => cc.CourseId == c.Id)
                    .Sum(cc => (decimal?)cc.Amount) ?? 0m,
            }
        ).ToListAsync(cancellationToken);

        return rows
            .Select(r => new CourseDto(r.Id, r.ClassName, r.Organizer, r.EventDateTime, r.DurationMinutes, r.TotalCreditAmount))
            .ToList();
    }

    public async Task<bool> UpdateAsync(int id, UpdateCourseInput input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Pre-validate by building a throwaway Course — Guard throws before we touch the DB row.
        _ = Course.Create(input.ClassName, input.Description, input.Organizer, input.EventDateTime, input.DurationMinutes);

        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (course is null)
            return false;

        ApplyInputToCourse(course, input.ClassName, input.Description, input.Organizer, input.EventDateTime, input.DurationMinutes);

        var existingLinks = await _db.ClassCredits.Where(cc => cc.CourseId == id).ToListAsync(cancellationToken);
        _db.ClassCredits.RemoveRange(existingLinks);

        var resolved = await ResolveCreditAssignmentsAsync(input.Credits, cancellationToken);
        foreach (var (creditId, amount) in resolved)
        {
            _db.ClassCredits.Add(ClassCredit.Create(id, creditId, amount));
        }

        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var course = await _db.Courses.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (course is null)
            return false;

        var links = await _db.ClassCredits.Where(cc => cc.CourseId == id).ToListAsync(cancellationToken);
        _db.ClassCredits.RemoveRange(links);
        _db.Courses.Remove(course);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // Course exposes private setters via factory only — set fields through the EF entry
    // so we don't have to add a mutation method on the domain entity yet (Phase 2 minimum).
    private void ApplyInputToCourse(
        Course course,
        string className,
        string description,
        string organizer,
        DateTimeOffset eventDateTime,
        int durationMinutes)
    {
        var entry = _db.Entry(course);
        entry.Property(nameof(Course.ClassName)).CurrentValue = className;
        entry.Property(nameof(Course.Description)).CurrentValue = description ?? string.Empty;
        entry.Property(nameof(Course.Organizer)).CurrentValue = organizer;
        entry.Property(nameof(Course.EventDateTime)).CurrentValue = eventDateTime;
        entry.Property(nameof(Course.DurationMinutes)).CurrentValue = durationMinutes;
        entry.Property(nameof(Course.UpdatedAt)).CurrentValue = DateTimeOffset.UtcNow;
    }

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
