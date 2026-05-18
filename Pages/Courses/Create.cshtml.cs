using System.ComponentModel.DataAnnotations;
using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Application.Credits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoctorPortfolioSite.Pages.Courses;

[Authorize(Roles = "Admin,Organizer")]
public class CreateModel : PageModel
{
    private readonly ICourseService _courseService;
    private readonly ICreditService _creditService;

    public CreateModel(ICourseService courseService, ICreditService creditService)
    {
        _courseService = courseService;
        _creditService = creditService;
    }

    [BindProperty]
    public CreateCourseInputModel Input { get; set; } = new();

    public IReadOnlyList<CreditOption> CreditOptions { get; private set; } = Array.Empty<CreditOption>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        CreditOptions = await _creditService.ListAsync(cancellationToken);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        CreditOptions = await _creditService.ListAsync(cancellationToken);

        if (!ModelState.IsValid)
            return Page();

        var eventDateTime = new DateTimeOffset(
            Input.EventDate.Year, Input.EventDate.Month, Input.EventDate.Day,
            Input.EventTime.Hour, Input.EventTime.Minute, 0,
            TimeSpan.Zero);

        var assignments = (Input.CreditCodes ?? Array.Empty<string>())
            .Select(code => new CreditAssignment(code, Input.CreditAmounts.TryGetValue(code, out var amount) ? amount : 1.0m))
            .ToList();

        try
        {
            await _courseService.CreateAsync(new CreateCourseInput(
                Input.ClassName,
                Input.Description ?? string.Empty,
                Input.Organizer,
                eventDateTime,
                Input.DurationMinutes,
                assignments), cancellationToken);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }

        TempData["SuccessMessage"] = $"クラス「{Input.ClassName}」を登録しました。";
        return RedirectToPage("./Index");
    }

    public class CreateCourseInputModel
    {
        [Required(ErrorMessage = "クラス名は必須です。")]
        [StringLength(200)]
        public string ClassName { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "開催団体は必須です。")]
        [StringLength(200)]
        public string Organizer { get; set; } = string.Empty;

        [Required(ErrorMessage = "開催日は必須です。")]
        [DataType(DataType.Date)]
        public DateOnly EventDate { get; set; }

        [Required(ErrorMessage = "開催時刻は必須です。")]
        [DataType(DataType.Time)]
        public TimeOnly EventTime { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "時間(分)は 1 以上を指定してください。")]
        public int DurationMinutes { get; set; }

        public string[]? CreditCodes { get; set; }

        public Dictionary<string, decimal> CreditAmounts { get; set; } = new();
    }
}
