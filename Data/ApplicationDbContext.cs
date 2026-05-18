using System.Reflection;
using DoctorPortfolioSite.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoctorPortfolioSite.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Credit> Credits => Set<Credit>();
    public DbSet<ClassCredit> ClassCredits => Set<ClassCredit>();
    public DbSet<CourseCompletion> CourseCompletions => Set<CourseCompletion>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<QrToken> QrTokens => Set<QrToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
