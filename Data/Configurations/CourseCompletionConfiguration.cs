using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorPortfolioSite.Data.Configurations;

internal sealed class CourseCompletionConfiguration : IEntityTypeConfiguration<CourseCompletion>
{
    public void Configure(EntityTypeBuilder<CourseCompletion> builder)
    {
        builder.Property(cc => cc.UserId).IsRequired().HasMaxLength(450);
        builder.HasOne<Course>().WithMany().HasForeignKey(cc => cc.CourseId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(cc => cc.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(cc => new { cc.UserId, cc.CourseId }).IsUnique();
    }
}
