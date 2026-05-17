using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.Seeding;

public class RoleSeederTests
{
    [Fact]
    public async Task SeedAsync_Creates_All_Three_Default_Roles_When_None_Present()
    {
        await using var provider = SeederTestHost.Build(s => s.AddScoped<RoleSeeder>());
        using var scope = provider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await seeder.SeedAsync();

        foreach (var role in RoleSeeder.DefaultRoles)
        {
            (await roleManager.RoleExistsAsync(role)).Should().BeTrue($"role '{role}' should be seeded");
        }
    }
}
