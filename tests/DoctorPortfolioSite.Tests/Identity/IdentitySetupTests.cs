using DoctorPortfolioSite.Domain.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace DoctorPortfolioSite.Tests.Identity;

[Trait("Category", "integration")]
public class IdentitySetupTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IdentitySetupTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task UserManager_Can_Create_And_Authenticate_User()
    {
        using var scope = _factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var unique = Guid.NewGuid().ToString("N");
        var user = new ApplicationUser
        {
            UserName = $"u-{unique}@example.com",
            Email = $"u-{unique}@example.com",
            Name = "Test User",
            MedicalLicenseNumber = $"LIC-{unique[..8]}",
        };
        const string password = "StrongP@ss1";

        try
        {
            var createResult = await userManager.CreateAsync(user, password);
            createResult.Succeeded.Should().BeTrue(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            var checkResult = await userManager.CheckPasswordAsync(user, password);
            checkResult.Should().BeTrue();

            var fetched = await userManager.FindByEmailAsync(user.Email!);
            fetched.Should().NotBeNull();
            fetched!.Name.Should().Be("Test User");
            fetched.MedicalLicenseNumber.Should().Be(user.MedicalLicenseNumber);
        }
        finally
        {
            var created = await userManager.FindByEmailAsync(user.Email!);
            if (created is not null)
            {
                await userManager.DeleteAsync(created);
            }
        }
    }
}
