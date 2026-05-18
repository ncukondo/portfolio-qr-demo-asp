using System.Net;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class AuthenticationFlowTests
{
    private const string TestEmail = "auth-test@example.com";
    private static string TestPassword => TestUserCredentials.Password;

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Anonymous_Access_To_Protected_Page_Redirects_To_Login()
    {
        await using var factory = new TestWebApplicationFactory();
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
        });

        var response = await client.GetAsync("/Protected/Sample");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Login");
    }

    [Fact]
    public async Task Login_Post_Issues_Cookie_And_Protected_Page_Becomes_Accessible()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateTestUserAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });

        var token = await GetAntiForgeryTokenAsync(client);

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Input.Email"] = TestEmail,
            ["Input.Password"] = TestPassword,
            ["Input.RememberMe"] = "false",
            ["__RequestVerificationToken"] = token,
        });
        var loginResponse = await client.PostAsync("/Identity/Account/Login", form);

        loginResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);

        var protectedResponse = await client.GetAsync("/Protected/Sample");
        protectedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await protectedResponse.Content.ReadAsStringAsync();
        body.Should().Contain("Protected Sample");
    }

    private static async Task CreateTestUserAsync(TestWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            UserName = TestEmail,
            Email = TestEmail,
            EmailConfirmed = true,
            Name = "Auth Test",
        };
        var result = await userManager.CreateAsync(user, TestPassword);
        result.Succeeded.Should().BeTrue(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    private static async Task<string> GetAntiForgeryTokenAsync(HttpClient client)
    {
        var loginPage = await client.GetAsync("/Identity/Account/Login");
        loginPage.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await loginPage.Content.ReadAsStringAsync();
        var match = AntiForgeryTokenRegex.Match(html);
        match.Success.Should().BeTrue("login page should expose an antiforgery token");
        return match.Groups["token"].Value;
    }
}
