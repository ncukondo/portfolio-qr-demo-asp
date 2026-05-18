using DoctorPortfolioSite.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DoctorPortfolioSite.Infrastructure.Seeding;

public class SampleUserSeeder
{
    // PHP roles → ASP.NET roles mapping (see RoleSeeder):
    //   administrator → Admin
    //   class-owner   → Organizer
    //   learner       → Participant
    public static readonly IReadOnlyList<SampleUser> DefaultUsers = new[]
    {
        new SampleUser("admin@example.com",    "Admin User",      new[] { "Admin" }),
        new SampleUser("owner@example.com",    "Owner User",      new[] { "Organizer" }),
        new SampleUser("learner1@example.com", "Learner One",     new[] { "Participant" }),
        new SampleUser("learner2@example.com", "Learner Two",     new[] { "Participant" }),
        new SampleUser("multi@example.com",    "Multi-role User", new[] { "Organizer", "Participant" }),
    };

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SeedingOptions _options;
    private readonly ILogger<SampleUserSeeder> _logger;

    public SampleUserSeeder(
        UserManager<ApplicationUser> userManager,
        IOptions<SeedingOptions> options,
        ILogger<SampleUserSeeder> logger)
    {
        _userManager = userManager;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var sample in DefaultUsers)
        {
            var user = await _userManager.FindByEmailAsync(sample.Email);
            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = sample.Email,
                    Email = sample.Email,
                    EmailConfirmed = true,
                    Name = sample.Name,
                };

                var createResult = await _userManager.CreateAsync(user, _options.SampleUserPassword);
                if (!createResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create sample user '{sample.Email}': {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var missingRoles = sample.Roles.Except(currentRoles, StringComparer.Ordinal).ToList();
            if (missingRoles.Count == 0)
                continue;

            var roleResult = await _userManager.AddToRolesAsync(user, missingRoles);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to assign roles to '{sample.Email}': {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
            }
        }
    }

    public sealed record SampleUser(string Email, string Name, IReadOnlyList<string> Roles);
}
