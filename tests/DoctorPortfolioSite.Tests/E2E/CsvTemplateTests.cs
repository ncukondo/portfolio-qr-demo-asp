using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class CsvTemplateTests
{
    private const string OrganizerEmail = "csv-organizer@example.com";
    private const string ParticipantEmail = "csv-participant@example.com";

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

        var response = await client.GetAsync("/Courses/CsvTemplate");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Redirect, HttpStatusCode.Found);
        response.Headers.Location!.ToString().Should().Contain("/Identity/Account/Login");
    }

    [Fact]
    public async Task Participant_Get_Is_Forbidden()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateUserAsync(factory, ParticipantEmail, "Participant");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        await LoginAsync(client, ParticipantEmail);

        var response = await client.GetAsync("/Courses/CsvTemplate");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Forbidden, HttpStatusCode.Redirect, HttpStatusCode.Found);
    }

    [Fact]
    public async Task Organizer_Get_Returns_Csv_With_Bom_And_Three_Lines()
    {
        await using var factory = new TestWebApplicationFactory();
        await CreateUserAsync(factory, OrganizerEmail, "Organizer");

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true,
        });
        await LoginAsync(client, OrganizerEmail);

        var response = await client.GetAsync("/Courses/CsvTemplate");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.ToString().Should().StartWith("text/csv");
        response.Content.Headers.ContentDisposition!.FileName.Should().StartWith("class_template_");
        response.Content.Headers.ContentDisposition.FileName.Should().EndWith(".csv");

        var bytes = await response.Content.ReadAsByteArrayAsync();
        // UTF-8 BOM: 0xEF 0xBB 0xBF
        bytes[0].Should().Be(0xEF);
        bytes[1].Should().Be(0xBB);
        bytes[2].Should().Be(0xBF);

        var text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        lines.Should().HaveCount(3); // header + 2 sample rows
        lines[0].Should().Contain("クラス名");
        lines[0].Should().Contain("単位コード");
        lines[1].Should().Contain("IT001");
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
        var loginPage = await client.GetAsync("/Identity/Account/Login");
        var token = AntiForgeryTokenRegex.Match(await loginPage.Content.ReadAsStringAsync()).Groups["token"].Value;
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
}
