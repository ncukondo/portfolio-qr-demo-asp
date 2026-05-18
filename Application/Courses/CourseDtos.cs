namespace DoctorPortfolioSite.Application.Courses;

public sealed record CourseDto(
    int Id,
    string ClassName,
    string Organizer,
    DateTimeOffset EventDateTime,
    int DurationMinutes,
    decimal TotalCreditAmount);

public sealed record CourseCreditDto(string Code, string Label, decimal Amount);

public sealed record CourseDetailsDto(
    int Id,
    string ClassName,
    string Description,
    string Organizer,
    DateTimeOffset EventDateTime,
    int DurationMinutes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<CourseCreditDto> Credits);

public sealed record CourseQuery(
    string? Organizer = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);

public sealed record CreditAssignment(string Code, decimal Amount);

public sealed record CreateCourseInput(
    string ClassName,
    string Description,
    string Organizer,
    DateTimeOffset EventDateTime,
    int DurationMinutes,
    IReadOnlyList<CreditAssignment> Credits);

public sealed record UpdateCourseInput(
    string ClassName,
    string Description,
    string Organizer,
    DateTimeOffset EventDateTime,
    int DurationMinutes,
    IReadOnlyList<CreditAssignment> Credits);
