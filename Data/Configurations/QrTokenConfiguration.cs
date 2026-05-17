using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorPortfolioSite.Data.Configurations;

internal sealed class QrTokenConfiguration : IEntityTypeConfiguration<QrToken>
{
    public void Configure(EntityTypeBuilder<QrToken> builder)
    {
        builder.Property(q => q.Token).IsRequired().HasMaxLength(200);
        builder.HasIndex(q => q.Token).IsUnique();
        builder.HasOne<Course>().WithMany().HasForeignKey(q => q.CourseId).OnDelete(DeleteBehavior.Cascade);
    }
}
