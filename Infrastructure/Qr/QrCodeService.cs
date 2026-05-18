using QRCoder;

namespace DoctorPortfolioSite.Infrastructure.Qr;

public class QrCodeService : IQrCodeService
{
    public string GenerateDataUrl(string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data);
        var bytes = png.GetGraphic(20);
        var base64 = Convert.ToBase64String(bytes);
        return $"data:image/png;base64,{base64}";
    }
}
