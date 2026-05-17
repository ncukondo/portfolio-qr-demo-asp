using DoctorPortfolioSite.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DoctorPortfolioSite.Infrastructure.Seeding;

public class AdminSeeder
{
    private const string AdminRole = "Admin";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdminSeedOptions _options;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        UserManager<ApplicationUser> userManager,
        IOptions<AdminSeedOptions> options,
        ILogger<AdminSeeder> logger)
    {
        _userManager = userManager;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
        {
            _logger.LogInformation("AdminSeeder skipped: Seed:Admin:Email or Password is not configured.");
            return;
        }

        var existing = await _userManager.FindByEmailAsync(_options.Email);
        if (existing is not null)
        {
            return;
        }

        var admin = new ApplicationUser
        {
            UserName = _options.Email,
            Email = _options.Email,
            EmailConfirmed = true,
            Name = string.IsNullOrWhiteSpace(_options.Name) ? "Admin" : _options.Name,
        };

        var createResult = await _userManager.CreateAsync(admin, _options.Password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }

        var roleResult = await _userManager.AddToRoleAsync(admin, AdminRole);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to assign Admin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
        }
    }
}
