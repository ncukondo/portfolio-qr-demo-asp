using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorPortfolioSite.Data.Configurations;

internal sealed class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.Venue).HasMaxLength(200);
        builder.Property(c => c.QrCodeSecret).IsRequired().HasMaxLength(200);
    }
}
