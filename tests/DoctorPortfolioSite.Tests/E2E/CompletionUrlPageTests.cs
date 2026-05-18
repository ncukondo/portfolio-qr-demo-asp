using System.Net;
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

public class CompletionUrlPageTests
{
    private const string OrganizerEmail = "url-organizer@example.com";

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Organizer_Get_Returns_Course_Checkboxes()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCoursesAsync(factory, "URL-Course-1", "URL-Course-2");
        await CreateOrganizerAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var response = await client.GetAsync("/Courses/CompletionUrl");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("URL-Course-1");
        html.Should().Contain("URL-Course-2");
    }

    [Fact]
    public async Task Organizer_Post_Valid_Selection_Renders_Url_And_Qr_Image()
    {
        await using var factory = new TestWebApplicationFactory();
        var ids = await SeedCoursesAsync(factory, "URL-Pick-A", "URL-Pick-B");
        await CreateOrganizerAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var token = await GetAntiForgeryTokenAsync(client, "/Courses/CompletionUrl");

        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Form.SelectedClassIds", ids[0].ToString()),
            new KeyValuePair<string, string>("Form.SelectedClassIds", ids[1].ToString()),
            new KeyValuePair<string, string>("Form.ExpirationHours", "2"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        });

        var response = await client.PostAsync("/Courses/CompletionUrl", form);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();

        html.Should().Contain("/Complete?token=");
        html.Should().Contain("data:image/png;base64,");
        html.Should().Contain("URL-Pick-A");
        html.Should().Contain("URL-Pick-B");
    }

    [Fact]
    public async Task Organizer_Post_With_No_Selection_Returns_Error()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCoursesAsync(factory, "X");
        await CreateOrganizerAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var token = await GetAntiForgeryTokenAsync(client, "/Courses/CompletionUrl");
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Form.ExpirationHours", "24"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        });

        var response = await client.PostAsync("/Courses/CompletionUrl", form);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().NotContain("data:image/png;base64,");
        html.Should().Contain("クラス");
    }

    [Fact]
    public async Task Organizer_Post_With_Invalid_ExpirationHours_Returns_Error()
    {
        await using var factory = new TestWebApplicationFactory();
        var ids = await SeedCoursesAsync(factory, "X");
        await CreateOrganizerAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var token = await GetAntiForgeryTokenAsync(client, "/Courses/CompletionUrl");
        var form = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("Form.SelectedClassIds", ids[0].ToString()),
            new KeyValuePair<string, string>("Form.ExpirationHours", "0"),
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
        });

        var response = await client.PostAsync("/Courses/CompletionUrl", form);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().NotContain("data:image/png;base64,");
    }

    private static async Task<int[]> SeedCoursesAsync(TestWebApplicationFactory factory, params string[] classNames)
    {
        using var scope = factory.Services.CreateScope();
        await new CreditSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();

        var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
        var ids = new List<int>();
        var anchor = new DateTimeOffset(2026, 6, 1, 4, 0, 0, TimeSpan.Zero);
        foreach (var name in classNames)
        {
            ids.Add(await courseService.CreateAsync(new CreateCourseInput(
                name, "d", "Org", anchor.AddHours(ids.Count), 60,
                new[] { new CreditAssignment("IT001", 1.0m) })));
        }
        return ids.ToArray();
    }

    private static async Task CreateOrganizerAsync(TestWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Organizer"))
            await roleManager.CreateAsync(new IdentityRole("Organizer"));

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = OrganizerEmail,
            Email = OrganizerEmail,
            EmailConfirmed = true,
            Name = OrganizerEmail,
        };
        (await userManager.CreateAsync(user, TestUserCredentials.Password)).Succeeded.Should().BeTrue();
        (await userManager.AddToRoleAsync(user, "Organizer")).Succeeded.Should().BeTrue();
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
