using DoctorPortfolioSite.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoctorPortfolioSite.Data.Configurations;

internal sealed class CreditConfiguration : IEntityTypeConfiguration<Credit>
{
    public void Configure(EntityTypeBuilder<Credit> builder)
    {
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Label).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Category).HasMaxLength(100);
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.HasIndex(c => c.Code).IsUnique();
    }
}
