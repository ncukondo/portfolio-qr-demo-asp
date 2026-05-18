using DoctorPortfolioSite.Application.Courses;
using FluentAssertions;

namespace DoctorPortfolioSite.Tests.Application.Courses;

public class CourseImportRowValidatorTests
{
    [Fact]
    public void Validate_With_Valid_Seven_Column_Row_Returns_Input()
    {
        var result = CourseImportRowValidator.Validate(new[]
        {
            "Course X", "desc", "Org Y", "2026-08-01", "13:00", "90", "IT001",
        });

        result.IsSuccess.Should().BeTrue();
        result.Input!.ClassName.Should().Be("Course X");
        result.Input.Organizer.Should().Be("Org Y");
        result.Input.DurationMinutes.Should().Be(90);
        result.Input.EventDateTime.Should().Be(new DateTimeOffset(2026, 8, 1, 13, 0, 0, TimeSpan.Zero));
        result.Input.Credits.Should().ContainSingle().Which.Code.Should().Be("IT001");
    }

    [Fact]
    public void Validate_With_Multiple_Credit_Columns_Picks_All_NonEmpty()
    {
        var result = CourseImportRowValidator.Validate(new[]
        {
            "C", "d", "O", "2026-08-01", "10:00", "60", "IT001", "BZ002", "", "  SK001 ",
        });

        result.IsSuccess.Should().BeTrue();
        result.Input!.Credits.Select(c => c.Code).Should().BeEquivalentTo(new[] { "IT001", "BZ002", "SK001" });
    }

    [Fact]
    public void Validate_With_Fewer_Than_Seven_Columns_Fails()
    {
        var result = CourseImportRowValidator.Validate(new[] { "x", "d", "o", "2026-08-01", "10:00", "60" });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("列数");
    }

    [Fact]
    public void Validate_With_Blank_ClassName_Fails()
    {
        var result = CourseImportRowValidator.Validate(new[]
        {
            "", "d", "Org", "2026-08-01", "10:00", "60", "IT001",
        });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("クラス名");
    }

    [Fact]
    public void Validate_With_Blank_Organizer_Fails()
    {
        var result = CourseImportRowValidator.Validate(new[]
        {
            "C", "d", "", "2026-08-01", "10:00", "60", "IT001",
        });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("開催団体");
    }

    [Fact]
    public void Validate_With_Bad_Date_Fails()
    {
        var result = CourseImportRowValidator.Validate(new[]
        {
            "C", "d", "Org", "nope", "10:00", "60", "IT001",
        });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("開催日");
    }

    [Fact]
    public void Validate_With_Bad_Time_Fails()
    {
        var result = CourseImportRowValidator.Validate(new[]
        {
            "C", "d", "Org", "2026-08-01", "25:00", "60", "IT001",
        });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("開催時刻");
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("abc")]
    public void Validate_With_Bad_Duration_Fails(string duration)
    {
        var result = CourseImportRowValidator.Validate(new[]
        {
            "C", "d", "Org", "2026-08-01", "10:00", duration, "IT001",
        });
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("時間");
    }
}
