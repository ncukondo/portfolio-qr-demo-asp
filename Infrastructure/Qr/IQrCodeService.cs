namespace DoctorPortfolioSite.Infrastructure.Qr;

public interface IQrCodeService
{
    string GenerateDataUrl(string payload);
}
