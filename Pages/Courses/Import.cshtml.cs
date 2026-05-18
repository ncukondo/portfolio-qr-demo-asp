using System.Text;
using DoctorPortfolioSite.Application.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DoctorPortfolioSite.Pages.Courses;

[Authorize(Roles = "Admin,Organizer")]
[RequestFormLimits(MultipartBodyLengthLimit = 5 * 1024 * 1024)]
[RequestSizeLimit(5 * 1024 * 1024)]
public class ImportModel : PageModel
{
    public const string ExpectedHeader =
        "クラス名,説明,開催団体,開催日,開催時刻,時間（分）,単位コード（カンマ区切り）";

    private readonly ICourseService _courseService;

    public ImportModel(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [BindProperty]
    public IFormFile? CsvFile { get; set; }

    public ImportResult? Result { get; private set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (CsvFile is null || CsvFile.Length == 0)
        {
            ModelState.AddModelError(nameof(CsvFile), "CSV ファイルを選択してください。");
            return Page();
        }

        if (!CsvFile.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(CsvFile), "拡張子が .csv のファイルのみ対応しています。");
            return Page();
        }

        string text;
        await using (var stream = CsvFile.OpenReadStream())
        using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
        {
            text = await reader.ReadToEndAsync(cancellationToken);
        }

        var lines = text
            .Replace("\r\n", "\n")
            .Split('\n', StringSplitOptions.None)
            .Where((line, index) => index == 0 || !string.IsNullOrWhiteSpace(line))
            .ToArray();

        if (lines.Length == 0)
        {
            Result = new ImportResult(HeaderError: "CSV が空です。", Rows: Array.Empty<RowResult>(), CreatedCourseIds: Array.Empty<int>());
            return Page();
        }

        var header = lines[0].TrimEnd('\r');
        if (!header.Equals(ExpectedHeader, StringComparison.Ordinal))
        {
            Result = new ImportResult(
                HeaderError: $"ヘッダが想定と異なります。期待: '{ExpectedHeader}'",
                Rows: Array.Empty<RowResult>(),
                CreatedCourseIds: Array.Empty<int>());
            return Page();
        }

        var rowResults = new List<RowResult>();
        var createdIds = new List<int>();

        for (var i = 1; i < lines.Length; i++)
        {
            var rawLine = lines[i];
            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            var columns = SplitCsvLine(rawLine);
            var validation = CourseImportRowValidator.Validate(columns);
            if (!validation.IsSuccess)
            {
                var attemptedName = columns.Length > 0 ? columns[0] : string.Empty;
                rowResults.Add(new RowResult(i, false, attemptedName, validation.Error ?? "不明なエラー", null));
                continue;
            }

            try
            {
                var id = await _courseService.CreateAsync(validation.Input!, cancellationToken);
                createdIds.Add(id);
                rowResults.Add(new RowResult(i, true, validation.Input!.ClassName, "登録成功", id));
            }
            catch (ArgumentException ex)
            {
                rowResults.Add(new RowResult(i, false, validation.Input!.ClassName, ex.Message, null));
            }
        }

        Result = new ImportResult(HeaderError: null, Rows: rowResults, CreatedCourseIds: createdIds);
        return Page();
    }

    // Naive CSV split: handles unquoted commas only. The PHP template has no
    // quoted values; if we need quoted-field support later, replace this with
    // a small parser rather than pulling in CsvHelper.
    private static string[] SplitCsvLine(string line) => line.TrimEnd('\r').Split(',');

    public sealed record ImportResult(
        string? HeaderError,
        IReadOnlyList<RowResult> Rows,
        IReadOnlyList<int> CreatedCourseIds);

    public sealed record RowResult(int LineNumber, bool IsSuccess, string ClassName, string Message, int? CourseId);
}
