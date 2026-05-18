using DoctorPortfolioSite.Application.Tokens;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace DoctorPortfolioSite.Tests.Application.Tokens;

public class ClassCompletionTokenServiceTests
{
    private const string Secret = "this-is-a-test-secret-32-bytes-min-length!!";
    private const string Issuer = "portfolio-system-test";

    private static ClassCompletionTokenService NewService(string? secret = null)
    {
        var opts = Options.Create(new JwtOptions { Secret = secret ?? Secret, Issuer = Issuer });
        return new ClassCompletionTokenService(opts);
    }

    [Fact]
    public void Generate_Then_TryDecode_RoundTrips_ClassIds()
    {
        var svc = NewService();
        var token = svc.Generate(new[] { 1, 2, 3 });

        var payload = svc.TryDecode(token);

        payload.Should().NotBeNull();
        payload!.ClassIds.Should().BeEquivalentTo(new[] { 1, 2, 3 });
        payload.Purpose.Should().Be("class_completion");
        payload.IssuedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
        payload.ExpiresAt.Should().BeAfter(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void Generate_Sets_Default_24_Hour_Expiration()
    {
        var svc = NewService();
        var token = svc.Generate(new[] { 1 });

        var payload = svc.TryDecode(token)!;

        var lifetime = payload.ExpiresAt - payload.IssuedAt;
        lifetime.TotalHours.Should().BeApproximately(24, 0.01);
    }

    [Fact]
    public void Generate_Honors_Custom_Expiration_Hours()
    {
        var svc = NewService();
        var token = svc.Generate(new[] { 1 }, expirationHours: 2);

        var payload = svc.TryDecode(token)!;

        (payload.ExpiresAt - payload.IssuedAt).TotalHours.Should().BeApproximately(2, 0.01);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(8761)]
    public void Generate_With_Out_Of_Range_Hours_Throws(int hours)
    {
        var svc = NewService();
        var act = () => svc.Generate(new[] { 1 }, hours);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Generate_With_Empty_ClassIds_Throws()
    {
        var svc = NewService();
        var act = () => svc.Generate(Array.Empty<int>());
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryDecode_With_Different_Secret_Returns_Null()
    {
        var issuer = NewService();
        var verifier = NewService("a-completely-different-secret-32-bytes-long!!");

        var token = issuer.Generate(new[] { 1 });
        var payload = verifier.TryDecode(token);

        payload.Should().BeNull();
    }

    [Fact]
    public void TryDecode_With_Garbage_String_Returns_Null()
    {
        var svc = NewService();
        svc.TryDecode("not-a-jwt").Should().BeNull();
        svc.TryDecode("").Should().BeNull();
    }

    [Fact]
    public void TryDecode_Of_Token_With_Wrong_Purpose_Returns_Null()
    {
        // Build a JWT manually with purpose set to something else, using the same secret.
        var token = ClassCompletionTokenTestHelpers.GenerateWithPurpose(Secret, Issuer, "something_else", new[] { 1 });

        var svc = NewService();
        svc.TryDecode(token).Should().BeNull();
    }

    [Fact]
    public void TryDecode_Of_Expired_Token_Returns_Null()
    {
        var token = ClassCompletionTokenTestHelpers.GenerateExpired(Secret, Issuer, new[] { 1 });

        var svc = NewService();
        svc.TryDecode(token).Should().BeNull();
    }

    [Fact]
    public void BuildCompletionUrl_Combines_Base_And_Encoded_Token()
    {
        var svc = NewService();

        var url = svc.BuildCompletionUrl(new[] { 1, 2 }, "https://example.test", expirationHours: 1);

        url.Should().StartWith("https://example.test/Complete?token=");
        var token = url.Substring("https://example.test/Complete?token=".Length);
        Uri.UnescapeDataString(token).Should().NotBeNullOrWhiteSpace();
        svc.TryDecode(Uri.UnescapeDataString(token)).Should().NotBeNull();
    }

    [Fact]
    public void BuildCompletionUrl_Strips_Trailing_Slash_From_Base()
    {
        var svc = NewService();
        var url = svc.BuildCompletionUrl(new[] { 1 }, "https://example.test/");
        url.Should().StartWith("https://example.test/Complete?token=");
        url.Should().NotContain("test//Complete");
    }
}
