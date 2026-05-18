using System.Net;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class DashboardNavTests
{
    private const string OrganizerEmail = "dash-org@example.com";
    private const string ParticipantEmail = "dash-part@example.com";

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Anonymous_Home_Shows_Login_Link_But_Not_Organizer_Actions()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();
        var html = await (await client.GetAsync("/")).Content.ReadAsStringAsync();

        html.Should().Contain("/Identity/Account/Login");
        html.Should().NotContain("クラス登録");
        html.Should().NotContain("CSV インポート");
    }

    [Fact]
    public async Task Participant_Home_Shows_MyCompletions_But_Not_Create()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateUserAsync(factory, ParticipantEmail, "Participant");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, ParticipantEmail);

        var html = await (await client.GetAsync("/")).Content.ReadAsStringAsync();

        html.Should().Contain("/MyCompletions");
        html.Should().NotContain("/Courses/Create");
        html.Should().NotContain("/Courses/Import");
    }

    [Fact]
    public async Task Organizer_Home_Shows_Create_And_Import_Links()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateUserAsync(factory, OrganizerEmail, "Organizer");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var html = await (await client.GetAsync("/")).Content.ReadAsStringAsync();

        html.Should().Contain("/Courses/Create");
        html.Should().Contain("/Courses/Import");
        html.Should().Contain("/Courses/CompletionUrl");
    }

    [Fact]
    public async Task Login_Page_In_Development_Shows_Demo_Accounts()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient();

        var html = await (await client.GetAsync("/Identity/Account/Login")).Content.ReadAsStringAsync();

        html.Should().Contain("admin@example.com");
        html.Should().Contain("owner@example.com");
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
