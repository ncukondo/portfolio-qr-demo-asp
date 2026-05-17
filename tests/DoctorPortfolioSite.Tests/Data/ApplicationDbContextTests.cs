using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Tests.Data;

public class ApplicationDbContextTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 4, 1, 9, 0, 0, TimeSpan.Zero);

    private static DbContextOptions<ApplicationDbContext> NewInMemoryOptions() =>
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

    [Fact]
    public async Task Can_Add_And_Query_Course()
    {
        var options = NewInMemoryOptions();
        var course = Course.Create("CME", "desc", 1, Anchor, Anchor.AddHours(1), "v", 10, "s", Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.Courses.Add(course);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.Courses.SingleAsync();
            loaded.Title.Should().Be("CME");
            loaded.Id.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Can_Add_And_Query_Enrollment()
    {
        var options = NewInMemoryOptions();
        var enrollment = Enrollment.Create("user-1", 100, Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.Enrollments.Add(enrollment);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.Enrollments.SingleAsync();
            loaded.UserId.Should().Be("user-1");
            loaded.Status.Should().Be(EnrollmentStatus.Enrolled);
        }
    }

    [Fact]
    public async Task Can_Add_And_Query_Portfolio()
    {
        var options = NewInMemoryOptions();
        var portfolio = Portfolio.Create("user-1", "Title", "desc", "goals", Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.Portfolios.Add(portfolio);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.Portfolios.SingleAsync();
            loaded.Title.Should().Be("Title");
            loaded.UserId.Should().Be("user-1");
        }
    }

    [Fact]
    public async Task Can_Add_And_Query_QrToken()
    {
        var options = NewInMemoryOptions();
        var token = QrToken.Create(100, "tok-abc", Anchor.AddMinutes(5), Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.QrTokens.Add(token);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.QrTokens.SingleAsync();
            loaded.Token.Should().Be("tok-abc");
            loaded.IsUsed.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Updating_Enrollment_State_Persists()
    {
        var options = NewInMemoryOptions();
        var enrollment = Enrollment.Create("user-1", 100, Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.Enrollments.Add(enrollment);
            await ctx.SaveChangesAsync();
            enrollment.MarkCompleted(Anchor.AddHours(2));
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.Enrollments.SingleAsync();
            loaded.Status.Should().Be(EnrollmentStatus.Completed);
            loaded.CompletedAt.Should().Be(Anchor.AddHours(2));
        }
    }
}
