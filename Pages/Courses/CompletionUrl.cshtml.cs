using System.ComponentModel.DataAnnotations;
using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Application.Tokens;
using DoctorPortfolioSite.Infrastructure.Qr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoctorPortfolioSite.Pages.Courses;

[Authorize(Roles = "Admin,Organizer")]
public class CompletionUrlModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly ICompletionTokenService _tokenService;
    private readonly IQrCodeService _qrCodeService;

    public CompletionUrlModel(
        ICourseService courseService,
        ICompletionTokenService tokenService,
        IQrCodeService qrCodeService)
    {
        _courseService = courseService;
        _tokenService = tokenService;
        _qrCodeService = qrCodeService;
    }

    public IReadOnlyList<CourseDto> Courses { get; private set; } = Array.Empty<CourseDto>();

    [BindProperty]
    public Input Form { get; set; } = new();

    public GeneratedResult? Result { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Courses = await _courseService.ListAsync(new CourseQuery(), cancellationToken);
        if (TempData["PrefilledClassIds"] is string prefilled && !string.IsNullOrWhiteSpace(prefilled))
        {
            Form.SelectedClassIds = prefilled
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var v) ? v : 0)
                .Where(v => v > 0)
                .ToArray();
        }
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        Courses = await _courseService.ListAsync(new CourseQuery(), cancellationToken);

        if (Form.SelectedClassIds is null || Form.SelectedClassIds.Length == 0)
            ModelState.AddModelError(nameof(Form.SelectedClassIds), "クラスを 1 つ以上選択してください。");

        if (!ModelState.IsValid)
            return Page();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        string url;
        try
        {
            url = _tokenService.BuildCompletionUrl(Form.SelectedClassIds!, baseUrl, Form.ExpirationHours);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        var qrDataUrl = _qrCodeService.GenerateDataUrl(url);

        var selectedNames = Courses
            .Where(c => Form.SelectedClassIds!.Contains(c.Id))
            .Select(c => c.ClassName)
            .ToArray();

        Result = new GeneratedResult(url, qrDataUrl, selectedNames);
        return Page();
    }

    public class Input
    {
        public int[]? SelectedClassIds { get; set; }

        [Range(1, 8760, ErrorMessage = "有効期限は 1〜8760 時間で指定してください。")]
        public int ExpirationHours { get; set; } = 24;
    }

    public sealed record GeneratedResult(string Url, string QrCodeDataUrl, IReadOnlyList<string> SelectedClassNames);
}
