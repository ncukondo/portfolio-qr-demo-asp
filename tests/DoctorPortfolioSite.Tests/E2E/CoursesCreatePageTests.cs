using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class CoursesCreatePageTests
{
    private const string OrganizerEmail = "create-organizer@example.com";
    private const string ParticipantEmail = "create-participant@example.com";

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Anonymous_Get_Redirects_To_Login()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Courses/Create");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
        response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Login");
    }

    [Fact]
    public async Task Participant_Get_Returns_403_Or_Redirect()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateUserAsync(factory, ParticipantEmail, "Participant");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        await LoginAsync(client, ParticipantEmail);

        var response = await client.GetAsync("/Courses/Create");

        // Identity may redirect to AccessDenied (302) or return 403 depending on config.
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Redirect, HttpStatusCode.Found);
        if (response.StatusCode != HttpStatusCode.Forbidden)
        {
            response.Headers.Location!.ToString().Should().Contain("AccessDenied");
        }
    }

    [Fact]
    public async Task Organizer_Get_Returns_200_With_Credit_Options()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCreditsAsync(factory);
        await CreateUserAsync(factory, OrganizerEmail, "Organizer");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        await LoginAsync(client, OrganizerEmail);

        var response = await client.GetAsync("/Courses/Create");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("IT001"); // seeded credit code rendered as checkbox option
        html.Should().Contain("__RequestVerificationToken");
    }

    [Fact]
    public async Task Organizer_Post_Valid_Form_Creates_Course_And_Redirects_To_Index()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCreditsAsync(factory);
        await CreateUserAsync(factory, OrganizerEmail, "Organizer");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        await LoginAsync(client, OrganizerEmail);

        var token = await GetAntiForgeryTokenAsync(client, "/Courses/Create");
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Input.ClassName", "テスト講座"),
            new KeyValuePair<string, string>("Input.Description", "詳細"),
            new KeyValuePair<string, string>("Input.Organizer", "Org"),
            new KeyValuePair<string, string>("Input.EventDate", "2026-08-01"),
            new KeyValuePair<string, string>("Input.EventTime", "13:00"),
            new KeyValuePair<string, string>("Input.DurationMinutes", "90"),
            new KeyValuePair<string, string>("Input.CreditCodes", "IT001"),
            new KeyValuePair<string, string>("Input.CreditAmounts[IT001]", "1.5"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        });

        var post = await client.PostAsync("/Courses/Create", form);

        post.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
        post.Headers.Location!.ToString().Should().Contain("/Courses");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var course = await db.Courses.SingleAsync(c => c.ClassName == "テスト講座");
        course.Organizer.Should().Be("Org");
        course.DurationMinutes.Should().Be(90);
        (await db.ClassCredits.CountAsync(cc => cc.CourseId == course.Id)).Should().Be(1);
    }

    [Fact]
    public async Task Organizer_Post_Invalid_DurationMinutes_Returns_200_With_Error()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCreditsAsync(factory);
        await CreateUserAsync(factory, OrganizerEmail, "Organizer");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        await LoginAsync(client, OrganizerEmail);

        var token = await GetAntiForgeryTokenAsync(client, "/Courses/Create");
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Input.ClassName", "壊れた講座"),
            new KeyValuePair<string, string>("Input.Description", ""),
            new KeyValuePair<string, string>("Input.Organizer", "Org"),
            new KeyValuePair<string, string>("Input.EventDate", "2026-08-01"),
            new KeyValuePair<string, string>("Input.EventTime", "13:00"),
            new KeyValuePair<string, string>("Input.DurationMinutes", "0"), // invalid
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        });

        var post = await client.PostAsync("/Courses/Create", form);

        post.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await post.Content.ReadAsStringAsync();
        html.Should().Contain("壊れた講座"); // input value preserved

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Courses.CountAsync(c => c.ClassName == "壊れた講座")).Should().Be(0);
    }

    private static async Task SeedCreditsAsync(TestWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        await new CreditSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
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
        var create = await userManager.CreateAsync(user, TestUserCredentials.Password);
        create.Succeeded.Should().BeTrue(string.Join(", ", create.Errors.Select(e => e.Description)));
        var assign = await userManager.AddToRoleAsync(user, role);
        assign.Succeeded.Should().BeTrue();
    }

    private static async Task LoginAsync(HttpClient client, string email)
    {
        var token = await GetAntiForgeryTokenAsync(client, "/Identity/Account/Login");
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = TestUserCredentials.Password,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
        });
        var login = await client.PostAsync("/Identity/Account/Login", form);
        login.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found, HttpStatusCode.OK);
    }

    private static async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string url)
    {
        var page = await client.GetAsync(url);
        page.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await page.Content.ReadAsStringAsync();
        var match = AntiForgeryTokenRegex.Match(html);
        match.Success.Should().BeTrue($"page {url} should expose an antiforgery token");
        return match.Groups["token"].Value;
    }
}
