using DoctorPortfolioSite.Infrastructure.Qr;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Infrastructure;

public class QrCodeServiceTests
{
    [Fact]
    public void GenerateDataUrl_Returns_Png_Base64_Data_Url()
    {
        var svc = new QrCodeService();
        var dataUrl = svc.GenerateDataUrl("https://example.test/Complete?token=abc");

        dataUrl.Should().StartWith("data:image/png;base64,");
        var base64 = dataUrl.Substring("data:image/png;base64,".Length);
        var bytes = Convert.FromBase64String(base64);
        bytes.Length.Should().BeGreaterThan(100); // png is non-trivial
        // PNG signature: 89 50 4E 47 0D 0A 1A 0A
        bytes[0].Should().Be(0x89);
        bytes[1].Should().Be((byte)'P');
        bytes[2].Should().Be((byte)'N');
        bytes[3].Should().Be((byte)'G');
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateDataUrl_Rejects_Blank_Payload(string blank)
    {
        var svc = new QrCodeService();
        var act = () => svc.GenerateDataUrl(blank);
        act.Should().Throw<ArgumentException>();
    }
}
