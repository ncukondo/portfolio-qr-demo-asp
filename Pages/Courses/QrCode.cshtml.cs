using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Application.Tokens;
using DoctorPortfolioSite.Infrastructure.Qr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoctorPortfolioSite.Pages.Courses;

[Authorize(Roles = "Admin,Organizer")]
public class QrCodeModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly ICompletionTokenService _tokenService;
    private readonly IQrCodeService _qrCodeService;

    public QrCodeModel(
        ICourseService courseService,
        ICompletionTokenService tokenService,
        IQrCodeService qrCodeService)
    {
        _courseService = courseService;
        _tokenService = tokenService;
        _qrCodeService = qrCodeService;
    }

    public CourseDetailsDto Course { get; private set; } = default!;
    public string CompletionUrl { get; private set; } = string.Empty;
    public string QrCodeDataUrl { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        var details = await _courseService.GetByIdAsync(id, cancellationToken);
        if (details is null)
            return NotFound();

        Course = details;
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        CompletionUrl = _tokenService.BuildCompletionUrl(new[] { id }, baseUrl);
        QrCodeDataUrl = _qrCodeService.GenerateDataUrl(CompletionUrl);
        return Page();
    }
}
