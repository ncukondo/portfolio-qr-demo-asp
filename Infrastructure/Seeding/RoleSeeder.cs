using Microsoft.AspNetCore.Identity;

namespace DoctorPortfolioSite.Infrastructure.Seeding;

public class RoleSeeder
{
    public static readonly IReadOnlyList<string> DefaultRoles = new[] { "Admin", "Organizer", "Participant" };

    private readonly RoleManager<IdentityRole> _roleManager;

    public RoleSeeder(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var role in DefaultRoles)
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(role));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to seed role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
    }
}
