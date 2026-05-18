using System.Security.Claims;
using DoctorPortfolioSite.Application.CourseCompletions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoctorPortfolioSite.Pages;

[Authorize]
public class MyCompletionsModel : PageModel
{
    private readonly ICourseCompletionService _completionService;

    public MyCompletionsModel(ICourseCompletionService completionService)
    {
        _completionService = completionService;
    }

    public IReadOnlyList<UserCompletionRow> Completions { get; private set; } = Array.Empty<UserCompletionRow>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
            return;

        Completions = await _completionService.GetUserCompletionsAsync(userId, cancellationToken);
    }
}
