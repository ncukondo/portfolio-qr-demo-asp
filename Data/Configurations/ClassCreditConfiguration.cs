using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorPortfolioSite.Data.Configurations;

internal sealed class ClassCreditConfiguration : IEntityTypeConfiguration<ClassCredit>
{
    public void Configure(EntityTypeBuilder<ClassCredit> builder)
    {
        builder.Property(cc => cc.Amount)
            .HasColumnName("credit_amount")
            .HasColumnType("decimal(3,1)")
            .HasDefaultValue(1.0m);

        builder.HasOne<Course>()
            .WithMany(c => c.Credits)
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Credit>()
            .WithMany()
            .HasForeignKey(cc => cc.CreditId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cc => new { cc.CourseId, cc.CreditId }).IsUnique();
    }
}
