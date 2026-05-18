using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorPortfolioSite.Data.Configurations;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.Property(c => c.ClassName).IsRequired().HasMaxLength(200).HasColumnName("class_name");
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.Organizer).IsRequired().HasMaxLength(200).HasColumnName("organizer");
        builder.Property(c => c.EventDateTime).HasColumnName("event_datetime");
        builder.Property(c => c.DurationMinutes).HasColumnName("duration_minutes");
    }
}
