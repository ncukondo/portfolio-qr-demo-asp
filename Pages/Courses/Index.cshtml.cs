using DoctorPortfolioSite.Application.Courses;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoctorPortfolioSite.Pages.Courses;

public class IndexModel : PageModel
{
    private readonly ICourseService _courseService;

    public IndexModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    public IReadOnlyList<CourseListItem> Courses { get; private set; } = Array.Empty<CourseListItem>();

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var courses = await _courseService.ListAsync(new CourseQuery(), cancellationToken);
        var items = new List<CourseListItem>(courses.Count);
        foreach (var c in courses)
        {
            var details = await _courseService.GetByIdAsync(c.Id, cancellationToken);
            items.Add(new CourseListItem(c, details?.Credits ?? Array.Empty<CourseCreditDto>(), details?.Description ?? string.Empty));
        }
        Courses = items;
    }

    public sealed record CourseListItem(CourseDto Summary, IReadOnlyList<CourseCreditDto> Credits, string Description);
}
