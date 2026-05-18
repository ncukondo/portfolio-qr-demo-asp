using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.Seeding;

public class SampleCourseSeederTests
{
    private static ServiceProvider BuildProvider() => SeederTestHost.Build(s =>
    {
        s.AddScoped<CreditSeeder>();
        s.AddScoped<SampleCourseSeeder>();
    });

    [Fact]
    public async Task SeedAsync_Inserts_Five_Sample_Courses_With_Credit_Links()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CreditSeeder>().SeedAsync();

        await scope.ServiceProvider.GetRequiredService<SampleCourseSeeder>().SeedAsync();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Courses.CountAsync()).Should().Be(SampleCourseSeeder.DefaultCourses.Count);
        (await db.ClassCredits.CountAsync()).Should().Be(SampleCourseSeeder.DefaultCourses.Sum(c => c.Credits.Count));
    }

    [Fact]
    public async Task SeedAsync_Is_Idempotent_When_Called_Twice()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CreditSeeder>().SeedAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<SampleCourseSeeder>();
        await seeder.SeedAsync();
        await seeder.SeedAsync();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Courses.CountAsync()).Should().Be(SampleCourseSeeder.DefaultCourses.Count);
        (await db.ClassCredits.CountAsync()).Should().Be(SampleCourseSeeder.DefaultCourses.Sum(c => c.Credits.Count));
    }

    [Fact]
    public async Task SeedAsync_Throws_When_Required_Credits_Are_Missing()
    {
        await using var provider = BuildProvider();
        using var scope = provider.CreateScope();
        // Intentionally skip CreditSeeder so the seeder cannot resolve credit codes.

        var seeder = scope.ServiceProvider.GetRequiredService<SampleCourseSeeder>();
        var act = async () => await seeder.SeedAsync();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
