using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DoctorPortfolioSite.Tests.Seeding;

public class AdminSeederTests
{
    [Fact]
    public async Task SeedAsync_Skips_When_Configuration_Is_Empty()
    {
        var options = new AdminSeedOptions();
        await using var provider = SeederTestHost.Build(s =>
        {
            s.AddScoped<RoleSeeder>();
            s.AddScoped<AdminSeeder>();
            s.AddSingleton<IOptions<AdminSeedOptions>>(Options.Create(options));
        });
        using var scope = provider.CreateScope();
        var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
        await roleSeeder.SeedAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        await seeder.SeedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        userManager.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task SeedAsync_Creates_Admin_And_Assigns_Admin_Role_When_Configured()
    {
        var options = new AdminSeedOptions
        {
            Email = "admin@example.com",
            Password = "StrongP@ss1",
            Name = "Initial Admin",
        };
        await using var provider = SeederTestHost.Build(s =>
        {
            s.AddScoped<RoleSeeder>();
            s.AddScoped<AdminSeeder>();
            s.AddSingleton<IOptions<AdminSeedOptions>>(Options.Create(options));
        });
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RoleSeeder>().SeedAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        await seeder.SeedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = await userManager.FindByEmailAsync("admin@example.com");
        admin.Should().NotBeNull();
        admin!.Name.Should().Be("Initial Admin");

        var roles = await userManager.GetRolesAsync(admin);
        roles.Should().Contain("Admin");
    }

    [Fact]
    public async Task SeedAsync_Is_Idempotent_When_Admin_Already_Exists()
    {
        var options = new AdminSeedOptions
        {
            Email = "admin@example.com",
            Password = "StrongP@ss1",
            Name = "Initial Admin",
        };
        await using var provider = SeederTestHost.Build(s =>
        {
            s.AddScoped<RoleSeeder>();
            s.AddScoped<AdminSeeder>();
            s.AddSingleton<IOptions<AdminSeedOptions>>(Options.Create(options));
        });
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RoleSeeder>().SeedAsync();
        var seeder = scope.ServiceProvider.GetRequiredService<AdminSeeder>();
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        userManager.Users.Count(u => u.Email == "admin@example.com").Should().Be(1);
    }
}
