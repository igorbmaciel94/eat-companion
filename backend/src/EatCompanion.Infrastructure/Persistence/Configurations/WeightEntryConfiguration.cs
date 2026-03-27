using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class WeightEntryConfiguration : IEntityTypeConfiguration<WeightEntry>
{
    public void Configure(EntityTypeBuilder<WeightEntry> builder)
    {
        builder.ToTable("weight_entries");

        builder.HasKey(we => we.Id);

        builder.Property(we => we.Date)
            .IsRequired();

        builder.Property(we => we.WeightKg)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(we => we.CreatedAt)
            .IsRequired();

        builder.HasIndex(we => new { we.UserId, we.Date })
            .IsUnique();
    }
}
