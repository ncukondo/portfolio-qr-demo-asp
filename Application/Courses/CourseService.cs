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

    public Task<CourseDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

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
