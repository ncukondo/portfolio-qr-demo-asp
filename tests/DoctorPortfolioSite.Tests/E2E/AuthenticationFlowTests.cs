using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DoctorPortfolioSite.Tests.E2E;

public class AuthenticationFlowTests
{
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
}
