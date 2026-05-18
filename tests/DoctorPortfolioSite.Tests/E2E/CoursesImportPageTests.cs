using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.E2E;

public class CoursesImportPageTests
{
    private const string OrganizerEmail = "import-organizer@example.com";

    private const string ExpectedHeader =
        "クラス名,説明,開催団体,開催日,開催時刻,時間（分）,単位コード（カンマ区切り）";

    private static readonly Regex AntiForgeryTokenRegex = new(
        @"name=""__RequestVerificationToken""[^>]*value=""(?<token>[^""]+)""",
        RegexOptions.Compiled);

    [Fact]
    public async Task Organizer_Post_Valid_Csv_Imports_All_Rows()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCreditsAsync(factory);
        await CreateOrganizerAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var token = await GetAntiForgeryTokenAsync(client, "/Courses/Import");

        var csv = ExpectedHeader + "\n" +
                  "Import-Course-A,desc-a,Org A,2026-08-01,10:00,60,IT001\n" +
                  "Import-Course-B,desc-b,Org B,2026-08-02,11:30,90,BZ001,IT002\n";

        var content = BuildMultipartCsv(csv, token);
        var response = await client.PostAsync("/Courses/Import", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Import-Course-A");
        html.Should().Contain("Import-Course-B");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Courses.CountAsync(c => c.ClassName.StartsWith("Import-Course-"))).Should().Be(2);
        var b = await db.Courses.Include(c => c.Credits).SingleAsync(c => c.ClassName == "Import-Course-B");
        b.Credits.Should().HaveCount(2);
    }

    [Fact]
    public async Task Organizer_Post_Csv_With_Wrong_Header_Returns_Error_And_Imports_Nothing()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCreditsAsync(factory);
        await CreateOrganizerAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var token = await GetAntiForgeryTokenAsync(client, "/Courses/Import");

        var csv = "wrong,header,row\nImport-Bad,d,Org,2026-08-01,10:00,60,IT001\n";

        var content = BuildMultipartCsv(csv, token);
        var response = await client.PostAsync("/Courses/Import", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("ヘッダ");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Courses.CountAsync(c => c.ClassName == "Import-Bad")).Should().Be(0);
    }

    [Fact]
    public async Task Organizer_Post_Csv_With_Mixed_Rows_Imports_Valid_And_Reports_Errors()
    {
        await using var factory = new TestWebApplicationFactory();
        await SeedCreditsAsync(factory);
        await CreateOrganizerAsync(factory);

        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await LoginAsync(client, OrganizerEmail);

        var token = await GetAntiForgeryTokenAsync(client, "/Courses/Import");

        var csv = ExpectedHeader + "\n" +
                  "Mixed-OK,d,Org,2026-08-01,10:00,60,IT001\n" +
                  ",d,Org,2026-08-01,10:00,60,IT001\n" + // blank class name
                  "Mixed-Bad-Time,d,Org,2026-08-01,99:99,60,IT001\n";

        var content = BuildMultipartCsv(csv, token);
        var response = await client.PostAsync("/Courses/Import", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var html = await response.Content.ReadAsStringAsync();
        html.Should().Contain("Mixed-OK");
        html.Should().Contain("クラス名"); // error message for blank class name row

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await db.Courses.CountAsync(c => c.ClassName == "Mixed-OK")).Should().Be(1);
        (await db.Courses.CountAsync(c => c.ClassName == "Mixed-Bad-Time")).Should().Be(0);
    }

    private static MultipartFormDataContent BuildMultipartCsv(string csvBody, string antiforgeryToken)
    {
        // Prepend UTF-8 BOM to mimic Excel exports.
        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = preamble.Concat(Encoding.UTF8.GetBytes(csvBody)).ToArray();

        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");

        var form = new MultipartFormDataContent
        {
            { new StringContent(antiforgeryToken), "__RequestVerificationToken" },
        };
        form.Add(fileContent, "CsvFile", "import.csv");
        return form;
    }

    private static async Task SeedCreditsAsync(TestWebApplicationFactory factory)
    {
        using var scope = factory.Services.CreateScope();
        await new CreditSeeder(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>()).SeedAsync();
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
