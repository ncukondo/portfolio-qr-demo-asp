using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class CoursesIndexPageTests
{
    private static readonly DateTimeOffset Anchor = new(2026, 6, 1, 4, 0, 0, TimeSpan.Zero);

    private const string UniqueCourseName = "E2E Sample Course";
    private const string OrganizerEmail = "organizer-e2e@example.com";
    private static string Password => TestUserCredentials.Password;

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Anonymous_Get_Returns_200_With_Seeded_Course_Listed()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCourseAsync(factory);

        var client = factory.CreateClient();

        var response = await client.GetAsync("/Courses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain(UniqueCourseName);
    }

    [Fact]
    public async Task Anonymous_Does_Not_See_Organizer_Only_Actions()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCourseAsync(factory);

        var client = factory.CreateClient();
        var html = await (await client.GetAsync("/Courses")).Content.ReadAsStringAsync();

        html.Should().NotContain("クラス登録");
        html.Should().NotContain("CSV インポート");
    }

    [Fact]
    public async Task Organizer_Sees_Create_And_Import_Links()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCourseAsync(factory);
        await CreateOrganizerUserAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        await LoginAsync(client, OrganizerEmail, Password);

        var html = await (await client.GetAsync("/Courses")).Content.ReadAsStringAsync();

        html.Should().Contain("クラス登録");
        html.Should().Contain("CSV インポート");
    }

    private static async Task SeedCourseAsync(TestWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var creditSeeder = new CreditSeeder(scope.ServiceProvider.GetRequiredService<DoctorPortfolioSite.Data.ApplicationDbContext>());
        await creditSeeder.SeedAsync();

        var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
        await courseService.CreateAsync(new CreateCourseInput(
            ClassName: UniqueCourseName,
            Description: "e2e desc",
            Organizer: "e2e org",
            EventDateTime: Anchor,
            DurationMinutes: 90,
            Credits: new[] { new CreditAssignment("IT001", 1.0m) }));
    }

    private static async Task CreateOrganizerUserAsync(TestWebApplicationFactory factory)
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
            Name = "Organizer E2E",
        };
        var create = await userManager.CreateAsync(user, Password);
        create.Succeeded.Should().BeTrue(string.Join(", ", create.Errors.Select(e => e.Description)));
        var role = await userManager.AddToRoleAsync(user, "Organizer");
        role.Succeeded.Should().BeTrue();
    }

    private static async Task LoginAsync(HttpClient client, string email, string password)
    {
        var loginPage = await client.GetAsync("/Identity/Account/Login");
        var token = AntiForgeryTokenRegex.Match(await loginPage.Content.ReadAsStringAsync()).Groups["token"].Value;

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = email,
            ["Input.Password"] = password,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
        });
        var login = await client.PostAsync("/Identity/Account/Login", form);
        login.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found, HttpStatusCode.OK);
    }
}
