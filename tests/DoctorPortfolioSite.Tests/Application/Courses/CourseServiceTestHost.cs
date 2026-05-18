using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Tests.Application.Courses;

internal sealed class CourseServiceTestHost : IAsyncDisposable
{
    public ApplicationDbContext Db { get; }
    public ICourseService Service { get; }

    private CourseServiceTestHost(ApplicationDbContext db, ICourseService service)
    {
        Db = db;
        Service = service;
    }

    public static async Task<CourseServiceTestHost> CreateAsync(bool seedCredits = true)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new ApplicationDbContext(options);

        if (seedCredits)
        {
            await new CreditSeeder(db).SeedAsync();
        }

        var service = new CourseService(db);
        return new CourseServiceTestHost(db, service);
    }

    public async ValueTask DisposeAsync() => await Db.DisposeAsync();
}
