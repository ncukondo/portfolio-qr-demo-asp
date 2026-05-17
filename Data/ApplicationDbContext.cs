using System.Reflection;
using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<QrToken> QrTokens => Set<QrToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
