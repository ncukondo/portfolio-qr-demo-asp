using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Application.CourseCompletions;
using DoctorPortfolioSite.Application.Tokens;
using DoctorPortfolioSite.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Pages;

public class CompleteModel : PageModel
{
    private readonly ICompletionTokenService _tokenService;
    private readonly ICourseCompletionService _completionService;
    private readonly ApplicationDbContext _db;

    public CompleteModel(
        ICompletionTokenService tokenService,
        ICourseCompletionService completionService,
        ApplicationDbContext db)
    {
        _tokenService = tokenService;
        _completionService = completionService;
        _db = db;
    }

    public PageStatus Status { get; private set; } = PageStatus.Ok;
    public string? ErrorMessage { get; private set; }
    public IReadOnlyList<RowResult> NewlyCompleted { get; private set; } = Array.Empty<RowResult>();
    public IReadOnlyList<RowResult> AlreadyCompleted { get; private set; } = Array.Empty<RowResult>();
    public IReadOnlyList<RowResult> NotFoundRows { get; private set; } = Array.Empty<RowResult>();

    public async Task<IActionResult> OnGetAsync(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            Status = PageStatus.Invalid;
            ErrorMessage = "トークンが指定されていません。";
            return Page();
        }

        var payload = _tokenService.TryDecode(token);
        if (payload is null)
        {
            Status = PageStatus.Invalid;
            ErrorMessage = "完了URL のトークンが無効です。再発行を依頼してください。";
            return Page();
        }

        // Token is valid; require auth here so we don't redirect on garbage tokens.
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            var returnUrl = Url.Page("/Complete", new { token });
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }

        if (!User.IsInRole("Participant"))
        {
            Status = PageStatus.Forbidden;
            ErrorMessage = "受講完了の登録は受講者ロールのみが行えます。";
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return Page();
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            Status = PageStatus.Invalid;
            ErrorMessage = "ユーザー情報を取得できませんでした。";
            return Page();
        }

        var newly = new List<RowResult>();
        var already = new List<RowResult>();
        var missing = new List<RowResult>();

        // Resolve class names for friendly display, in one query.
        var ids = payload.ClassIds.Distinct().ToArray();
        var courseLookup = await _db.Courses
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.ClassName })
            .ToDictionaryAsync(x => x.Id, x => x.ClassName, cancellationToken);

        foreach (var classId in ids)
        {
            var name = courseLookup.TryGetValue(classId, out var n) ? n : $"#{classId}";
            var result = await _completionService.RegisterAsync(userId, classId, cancellationToken: cancellationToken);
            switch (result)
            {
                case RegisterCompletionResult.Created:
                    newly.Add(new RowResult(classId, name));
                    break;
                case RegisterCompletionResult.AlreadyExists:
                    already.Add(new RowResult(classId, name));
                    break;
                case RegisterCompletionResult.CourseNotFound:
                    missing.Add(new RowResult(classId, name));
                    break;
            }
        }

        NewlyCompleted = newly;
        AlreadyCompleted = already;
        NotFoundRows = missing;
        return Page();
    }

    public enum PageStatus
    {
        Ok,
        Invalid,
        Forbidden,
    }

    public sealed record RowResult(int CourseId, string ClassName);
}
