using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Tests.Integration;

// TODO: When Docker is available in the test environment, migrate this to Testcontainers.PostgreSql
// for full test isolation. Currently uses the dev container's postgres service directly because the
// devcontainer does not enable Docker-in-Docker.
[Trait("Category", "integration")]
public class PostgresIntegrationTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    private static string ConnectionString =>
        Environment.GetEnvironmentVariable("POSTGRES_TEST_CONNECTION")
        ?? "Host=postgres;Database=portfoliodb;Username=portfoliouser;Password=portfoliopass";

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Can_Insert_And_Query_Course_Against_Postgres()
    {
        var uniqueName = $"IT-Course-{Guid.NewGuid()}";
        var course = Course.Create(uniqueName, "desc", "Org", Anchor, 60, Anchor);

        await using var ctx = CreateContext();
        ctx.Courses.Add(course);
        await ctx.SaveChangesAsync();

        try
        {
            course.Id.Should().BeGreaterThan(0);

            var loaded = await ctx.Courses.AsNoTracking().SingleAsync(c => c.Id == course.Id);
            loaded.ClassName.Should().Be(uniqueName);
        }
        finally
        {
            ctx.Courses.Remove(course);
            await ctx.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Credit_Code_Uniqueness_Is_Enforced_By_Postgres()
    {
        var sharedCode = $"DUP-{Guid.NewGuid()}";
        var first = Credit.Create(sharedCode, "Label", "Cat", "Desc", Anchor);

        await using var ctx = CreateContext();
        ctx.Credits.Add(first);
        await ctx.SaveChangesAsync();

        try
        {
            await using var ctx2 = CreateContext();
            var duplicate = Credit.Create(sharedCode, "Label2", "Cat", "Desc", Anchor);
            ctx2.Credits.Add(duplicate);

            var act = async () => await ctx2.SaveChangesAsync();
            await act.Should().ThrowAsync<DbUpdateException>();
        }
        finally
        {
            ctx.Credits.Remove(first);
            await ctx.SaveChangesAsync();
        }
    }
}
