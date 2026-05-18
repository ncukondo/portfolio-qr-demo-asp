using System.Net;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Application.Tokens;
using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class CompletePageTests
{
    private const string ParticipantEmail = "complete-participant@example.com";
    private const string OrganizerEmail = "complete-organizer@example.com";

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Get_With_Invalid_Token_Shows_Error()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Complete?token=not-a-jwt");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("alert-danger");
        html.Should().NotContain("新規受講完了");
        html.Should().NotContain("既に受講済み");
    }

    [Fact]
    public async Task Get_Without_Token_Shows_Error()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Complete");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("alert-danger");
        html.Should().NotContain("新規受講完了");
    }

    [Fact]
    public async Task Anonymous_With_Valid_Token_Redirects_To_Login_With_ReturnUrl()
    {
        await using var factory = new TestWebApplicationFactory();
        var (id, token) = await SeedAndIssueAsync(factory, "Complete-Course-Anon");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync($"/Complete?token={Uri.EscapeDataString(token)}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
        response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Login");
        response.Headers.Location.ToString().Should().Contain("returnUrl=");
    }

    [Fact]
    public async Task Participant_With_Valid_Token_Registers_Completion_And_Shows_Newly()
    {
        await using var factory = new TestWebApplicationFactory();
        var (id, token) = await SeedAndIssueAsync(factory, "Complete-Course-OK");
        await CreateUserAsync(factory, ParticipantEmail, "Participant");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, ParticipantEmail);

        var response = await client.GetAsync($"/Complete?token={Uri.EscapeDataString(token)}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Complete-Course-OK");
        html.Should().Contain("新規受講完了");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.CourseCompletions.CountAsync(cc => cc.CourseId == id)).Should().Be(1);
    }

    [Fact]
    public async Task Participant_Repeating_Same_Token_Shows_AlreadyCompleted()
    {
        await using var factory = new TestWebApplicationFactory();
        var (id, token) = await SeedAndIssueAsync(factory, "Complete-Course-Dup");
        await CreateUserAsync(factory, ParticipantEmail, "Participant");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, ParticipantEmail);

        await client.GetAsync($"/Complete?token={Uri.EscapeDataString(token)}");
        var second = await client.GetAsync($"/Complete?token={Uri.EscapeDataString(token)}");
        var html = await second.Content.ReadAsStringAsync();

        html.Should().Contain("既に受講済み");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.CourseCompletions.CountAsync(cc => cc.CourseId == id)).Should().Be(1);
    }

    [Fact]
    public async Task Organizer_With_Valid_Token_Is_Forbidden()
    {
        await using var factory = new TestWebApplicationFactory();
        var (_, token) = await SeedAndIssueAsync(factory, "Complete-Course-OrgBlock");
        await CreateUserAsync(factory, OrganizerEmail, "Organizer");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var response = await client.GetAsync($"/Complete?token={Uri.EscapeDataString(token)}");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static async Task<(int CourseId, string Token)> SeedAndIssueAsync(TestWebApplicationFactory factory, string className)
    {
        using var scope = factory.Services.CreateScope();
        await new CreditSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
        var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
        var id = await courseService.CreateAsync(new CreateCourseInput(
            className, "d", "Org",
            new DateTimeOffset(2026, 6, 1, 4, 0, 0, TimeSpan.Zero),
            60,
            new[] { new CreditAssignment("IT001", 1.0m) }));

        var token = scope.ServiceProvider.GetRequiredService<ICompletionTokenService>().Generate(new[] { id });
        return (id, token);
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
