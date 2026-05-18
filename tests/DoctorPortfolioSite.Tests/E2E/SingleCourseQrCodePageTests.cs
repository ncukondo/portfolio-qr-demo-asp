using System.Net;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class SingleCourseQrCodePageTests
{
    private const string OrganizerEmail = "single-qr-organizer@example.com";
    private const string ParticipantEmail = "single-qr-participant@example.com";

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Anonymous_Redirects_To_Login()
    {
        await using var factory = new TestWebApplicationFactory();
        var id = await SeedCourseAsync(factory, "Single-QR-Anon");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var response = await client.GetAsync($"/Courses/{id}/QrCode");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
        response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Login");
    }

    [Fact]
    public async Task Participant_Is_Forbidden()
    {
        await using var factory = new TestWebApplicationFactory();
        var id = await SeedCourseAsync(factory, "Single-QR-Part");
        await CreateUserAsync(factory, ParticipantEmail, "Participant");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true });
        await LoginAsync(client, ParticipantEmail);

        var response = await client.GetAsync($"/Courses/{id}/QrCode");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Redirect, HttpStatusCode.Found);
    }

    [Fact]
    public async Task Organizer_Returns_200_With_QrImage_And_ClassName()
    {
        await using var factory = new TestWebApplicationFactory();
        var id = await SeedCourseAsync(factory, "Single-QR-Course-OK");
        await CreateUserAsync(factory, OrganizerEmail, "Organizer");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var response = await client.GetAsync($"/Courses/{id}/QrCode");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Single-QR-Course-OK");
        html.Should().Contain("/Complete?token=");
        html.Should().Contain("data:image/png;base64,");
    }

    [Fact]
    public async Task Organizer_Missing_Course_Returns_404()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateUserAsync(factory, OrganizerEmail, "Organizer");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var response = await client.GetAsync("/Courses/9999/QrCode");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<int> SeedCourseAsync(TestWebApplicationFactory factory, string className)
    {
        using var scope = factory.Services.CreateScope();
        await new CreditSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
        var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
        return await courseService.CreateAsync(new CreateCourseInput(
            className, "d", "Org",
            new DateTimeOffset(2026, 6, 1, 4, 0, 0, TimeSpan.Zero),
            60,
            new[] { new CreditAssignment("IT001", 1.0m) }));
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
        var login = await client.GetAsync("/Identity/Account/Login");
        var token = AntiForgeryTokenRegex.Match(await login.Content.ReadAsStringAsync()).Groups["token"].Value;
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
