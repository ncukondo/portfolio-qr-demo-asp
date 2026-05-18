using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoctorPortfolioSite.Pages.Courses;

[Authorize(Roles = "Admin,Organizer")]
public class CsvTemplateModel : PageModel
{
    private const string Header =
        "クラス名,説明,開催団体,開催日,開催時刻,時間（分）,単位コード（カンマ区切り）";

    private static readonly string[] SampleRows =
    {
        "Web開発入門サンプル,HTML、CSS、JavaScriptの基礎を学ぶクラスのサンプル,技術研修センター,2024-12-01,10:00,120,IT001,IT002",
        "データベース設計サンプル,PostgreSQLを使用したデータベース設計とSQL基礎のサンプル,データベース研究室,2024-12-05,14:00,90,IT002,BZ002",
    };

    public IActionResult OnGet()
    {
        var body = new StringBuilder();
        body.AppendLine(Header);
        foreach (var row in SampleRows)
            body.AppendLine(row);

        var preamble = Encoding.UTF8.GetPreamble(); // UTF-8 BOM: EF BB BF
        var content = Encoding.UTF8.GetBytes(body.ToString());
        var bytes = new byte[preamble.Length + content.Length];
        Buffer.BlockCopy(preamble, 0, bytes, 0, preamble.Length);
        Buffer.BlockCopy(content, 0, bytes, preamble.Length, content.Length);

        var fileName = string.Format(
            CultureInfo.InvariantCulture,
            "class_template_{0:yyyy-MM-dd}.csv",
            DateTimeOffset.UtcNow);

        return File(bytes, "text/csv; charset=utf-8", fileName);
    }
}
