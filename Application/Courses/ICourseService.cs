namespace DoctorPortfolioSite.Application.Courses;

public interface ICourseService
{
    Task<int> CreateAsync(CreateCourseInput input, CancellationToken cancellationToken = default);

    Task<CourseDetailsDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CourseDto>> ListAsync(CourseQuery query, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(int id, UpdateCourseInput input, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
