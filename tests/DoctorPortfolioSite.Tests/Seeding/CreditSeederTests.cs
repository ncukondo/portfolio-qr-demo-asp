using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.Seeding;

public class CreditSeederTests
{
    [Fact]
    public async Task SeedAsync_Inserts_All_Eight_Default_Credits()
    {
        await using var provider = SeederTestHost.Build(s => s.AddScoped<CreditSeeder>());
        using var scope = provider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<CreditSeeder>();

        await seeder.SeedAsync();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var codes = await db.Credits.Select(c => c.Code).OrderBy(c => c).ToListAsync();
        codes.Should().BeEquivalentTo(new[] { "BZ001", "BZ002", "IT001", "IT002", "LG001", "LG002", "SK001", "SK002" });
    }

    [Fact]
    public async Task SeedAsync_Is_Idempotent_When_Called_Twice()
    {
        await using var provider = SeederTestHost.Build(s => s.AddScoped<CreditSeeder>());
        using var scope = provider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<CreditSeeder>();

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Credits.CountAsync()).Should().Be(8);
    }
}
