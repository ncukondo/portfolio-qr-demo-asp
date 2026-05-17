using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorPortfolioSite.Data.Configurations;

internal sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.Property(e => e.UserId).IsRequired().HasMaxLength(450);
        builder.HasOne<Course>().WithMany().HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => new { e.UserId, e.CourseId }).IsUnique();
    }
}
