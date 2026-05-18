using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DoctorPortfolioSite.Tests.Seeding;

public class SampleUserSeederTests
{
    private static ServiceProvider BuildProvider() => SeederTestHost.Build(s =>
    {
        s.AddSingleton<IOptions<SeedingOptions>>(Options.Create(new SeedingOptions { SampleUserPassword = "Password123!" }));
        s.AddScoped<RoleSeeder>();
        s.AddScoped<SampleUserSeeder>();
    });

    [Fact]
    public async Task SeedAsync_Creates_All_Five_Php_Users_With_Mapped_Roles()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RoleSeeder>().SeedAsync();

        await scope.ServiceProvider.GetRequiredService<SampleUserSeeder>().SeedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var expectations = new (string Email, string[] Roles)[]
        {
            ("admin@example.com", new[] { "Admin" }),
            ("owner@example.com", new[] { "Organizer" }),
            ("learner1@example.com", new[] { "Participant" }),
            ("learner2@example.com", new[] { "Participant" }),
            ("multi@example.com", new[] { "Organizer", "Participant" }),
        };

        foreach (var (email, expectedRoles) in expectations)
        {
            var user = await userManager.FindByEmailAsync(email);
            user.Should().NotBeNull($"sample user '{email}' should exist");

            var roles = await userManager.GetRolesAsync(user!);
            roles.Should().BeEquivalentTo(expectedRoles, $"roles assigned to '{email}'");
        }
    }

    [Fact]
    public async Task SeedAsync_Uses_Configured_Password_That_Verifies()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RoleSeeder>().SeedAsync();

        await scope.ServiceProvider.GetRequiredService<SampleUserSeeder>().SeedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync("admin@example.com");

        (await userManager.CheckPasswordAsync(user!, "Password123!")).Should().BeTrue();
    }

    [Fact]
    public async Task SeedAsync_Is_Idempotent_When_Called_Twice()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<RoleSeeder>().SeedAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<SampleUserSeeder>();
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        userManager.Users.Count().Should().Be(5);
    }
}
