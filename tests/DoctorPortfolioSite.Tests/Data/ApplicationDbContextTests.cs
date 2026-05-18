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
        var course = Course.Create("CME", "desc", "Organizer", Anchor, 60, Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.Courses.Add(course);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.Courses.SingleAsync();
            loaded.ClassName.Should().Be("CME");
            loaded.Organizer.Should().Be("Organizer");
            loaded.DurationMinutes.Should().Be(60);
            loaded.Id.Should().BeGreaterThan(0);
        }
    }

    [Fact]
    public async Task Can_Add_And_Query_Credit()
    {
        var options = NewInMemoryOptions();
        var credit = Credit.Create("IT001", "Internal", "Cat", "Desc", Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.Credits.Add(credit);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.Credits.SingleAsync();
            loaded.Code.Should().Be("IT001");
            loaded.Label.Should().Be("Internal");
        }
    }

    [Fact]
    public async Task Can_Add_And_Query_ClassCredit()
    {
        var options = NewInMemoryOptions();
        var course = Course.Create("CME", "desc", "Org", Anchor, 60, Anchor);
        var credit = Credit.Create("IT001", "Internal", "Cat", "Desc", Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.Courses.Add(course);
            ctx.Credits.Add(credit);
            await ctx.SaveChangesAsync();

            var classCredit = ClassCredit.Create(course.Id, credit.Id, 1.5m, Anchor);
            ctx.ClassCredits.Add(classCredit);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.ClassCredits.SingleAsync();
            loaded.CourseId.Should().Be(course.Id);
            loaded.CreditId.Should().Be(credit.Id);
            loaded.Amount.Should().Be(1.5m);
        }
    }

    [Fact]
    public async Task Can_Add_And_Query_CourseCompletion()
    {
        var options = NewInMemoryOptions();
        var completion = CourseCompletion.Create("user-1", 100, Anchor, Anchor);

        await using (var ctx = new ApplicationDbContext(options))
        {
            ctx.CourseCompletions.Add(completion);
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = new ApplicationDbContext(options))
        {
            var loaded = await ctx.CourseCompletions.SingleAsync();
            loaded.UserId.Should().Be("user-1");
            loaded.CourseId.Should().Be(100);
            loaded.CompletedAt.Should().Be(Anchor);
        }
    }
}
