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

        modelBuilder.Entity<Course>(e =>
        {
            e.Property(c => c.Title).IsRequired().HasMaxLength(200);
            e.Property(c => c.Description).HasMaxLength(2000);
            e.Property(c => c.Venue).HasMaxLength(200);
            e.Property(c => c.QrCodeSecret).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<Enrollment>(e =>
        {
            e.Property(en => en.UserId).IsRequired().HasMaxLength(450);
            e.HasOne<Course>().WithMany().HasForeignKey(en => en.CourseId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(en => new { en.UserId, en.CourseId }).IsUnique();
        });

        modelBuilder.Entity<Portfolio>(e =>
        {
            e.Property(p => p.UserId).IsRequired().HasMaxLength(450);
            e.Property(p => p.Title).IsRequired().HasMaxLength(200);
            e.Property(p => p.Description).HasMaxLength(2000);
            e.Property(p => p.LearningGoals).HasMaxLength(4000);
        });

        modelBuilder.Entity<QrToken>(e =>
        {
            e.Property(q => q.Token).IsRequired().HasMaxLength(200);
            e.HasIndex(q => q.Token).IsUnique();
            e.HasOne<Course>().WithMany().HasForeignKey(q => q.CourseId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
