using System.Net;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Application.CourseCompletions;
using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class MyCompletionsPageTests
{
    private const string ParticipantEmail = "mc-participant@example.com";
    private const string OtherEmail = "mc-other@example.com";

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Anonymous_Redirects_To_Login()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/MyCompletions");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
        response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Login");
    }

    [Fact]
    public async Task Participant_With_Zero_Completions_Shows_Empty_State()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateUserAsync(factory, ParticipantEmail, "Participant");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, ParticipantEmail);

        var response = await client.GetAsync("/MyCompletions");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("text-muted"); // empty-state paragraph
        html.Should().NotContain("<tbody>");
    }

    [Fact]
    public async Task Participant_Sees_Only_Their_Own_Completions_Newest_First()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateUserAsync(factory, ParticipantEmail, "Participant");
        await CreateUserAsync(factory, OtherEmail, "Participant");

        var (myId, otherId) = await GetUserIdsAsync(factory, ParticipantEmail, OtherEmail);
        var ids = await SeedCoursesAsync(factory, "MC-A", "MC-B", "MC-C-OTHER");

        // Register completions: self gets MC-A (older) and MC-B (newer), other user gets MC-C.
        using (var scope = factory.Services.CreateScope())
        {
            var svc = scope.ServiceProvider.GetRequiredService<ICourseCompletionService>();
            await svc.RegisterAsync(myId, ids[0], new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero));
            await svc.RegisterAsync(myId, ids[1], new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero));
            await svc.RegisterAsync(otherId, ids[2], new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero));
        }

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, ParticipantEmail);

        var response = await client.GetAsync("/MyCompletions");
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("MC-A");
        html.Should().Contain("MC-B");
        html.Should().NotContain("MC-C-OTHER");

        // MC-B was completed later (2026-06) than MC-A (2026-05) — should appear earlier in the table.
        var idxA = html.IndexOf("MC-A", StringComparison.Ordinal);
        var idxB = html.IndexOf("MC-B", StringComparison.Ordinal);
        idxB.Should().BeLessThan(idxA);
    }

    private static async Task<int[]> SeedCoursesAsync(TestWebApplicationFactory factory, params string[] names)
    {
        using var scope = factory.Services.CreateScope();
        await new CreditSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
        var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
        var ids = new List<int>();
        var anchor = new DateTimeOffset(2026, 6, 1, 4, 0, 0, TimeSpan.Zero);
        foreach (var name in names)
        {
            ids.Add(await courseService.CreateAsync(new CreateCourseInput(
                name, "d", "Org", anchor.AddHours(ids.Count), 60,
                new[] { new CreditAssignment("IT001", 1.0m) })));
        }
        return ids.ToArray();
    }

    private static async Task CreateUserAsync(TestWebApplicationFactory factory, string email, string role)
    {
        using var scope = factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            Name = email,
        };
        (await userManager.CreateAsync(user, TestUserCredentials.Password)).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(user, role)).Succeeded.Should().BeTrue();
    }

    private static async Task<(string MyId, string OtherId)> GetUserIdsAsync(TestWebApplicationFactory factory, string me, string other)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var myUser = await userManager.FindByEmailAsync(me);
        var otherUser = await userManager.FindByEmailAsync(other);
        return (myUser!.Id, otherUser!.Id);
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var page = await client.GetAsync("/Identity/Account/Login");
        var token = AntiForgeryTokenRegex.Match(await page.Content.ReadAsStringAsync()).Groups["token"].Value;
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = TestUserCredentials.Password,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
        });
        var resp = await client.PostAsync("/Identity/Account/Login", form);
        resp.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found, HttpStatusCode.OK);
    }
}
