using DoctorPortfolioSite.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoctorPortfolioSite.Pages;

public class IndexModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public IndexModel(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    public bool IsSignedIn { get; private set; }
    public IReadOnlyList<string> Roles { get; private set; } = Array.Empty<string>();
    public string? DisplayName { get; private set; }

    public bool IsAdminOrOrganizer => Roles.Contains("Admin") || Roles.Contains("Organizer");
    public bool IsParticipant => Roles.Contains("Participant");

    public async Task OnGetAsync()
    {
        IsSignedIn = _signInManager.IsSignedIn(User);
        if (!IsSignedIn)
            return;

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return;

        DisplayName = user.Name;
        Roles = (await _userManager.GetRolesAsync(user)).ToList();
    }
}
