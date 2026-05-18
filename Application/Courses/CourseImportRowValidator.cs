using System.Globalization;

namespace DoctorPortfolioSite.Application.Courses;

public static class CourseImportRowValidator
{
    public const int FixedColumnCount = 6;

    public static Result Validate(string[] columns)
    {
        if (columns.Length < FixedColumnCount + 1)
            return Result.Failure($"列数が不足しています (最低 {FixedColumnCount + 1} 列必要)");

        var className = columns[0]?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(className))
            return Result.Failure("クラス名が空です");

        var description = columns[1] ?? string.Empty;

        var organizer = columns[2]?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(organizer))
            return Result.Failure("開催団体が空です");

        var dateText = columns[3]?.Trim() ?? string.Empty;
        if (!DateOnly.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            return Result.Failure($"開催日が不正です: '{dateText}'");

        var timeText = columns[4]?.Trim() ?? string.Empty;
        if (!TimeOnly.TryParse(timeText, CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            return Result.Failure($"開催時刻が不正です: '{timeText}'");

        var durationText = columns[5]?.Trim() ?? string.Empty;
        if (!int.TryParse(durationText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var duration) || duration <= 0)
            return Result.Failure($"時間(分)が不正です: '{durationText}'");

        var creditCodes = columns
            .Skip(FixedColumnCount)
            .Select(c => c?.Trim() ?? string.Empty)
            .Where(c => !string.IsNullOrEmpty(c))
            .ToArray();

        var eventDateTime = new DateTimeOffset(
            date.Year, date.Month, date.Day,
            time.Hour, time.Minute, 0,
            TimeSpan.Zero);

        var assignments = creditCodes
            .Select(code => new CreditAssignment(code, 1.0m))
            .ToList();

        return Result.Success(new CreateCourseInput(
            className, description, organizer, eventDateTime, duration, assignments));
    }

    public sealed record Result(bool IsSuccess, string? Error, CreateCourseInput? Input)
    {
        public static Result Success(CreateCourseInput input) => new(true, null, input);
        public static Result Failure(string error) => new(false, error, null);
    }
}
