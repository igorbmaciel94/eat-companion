using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class MealOptionConfiguration : IEntityTypeConfiguration<MealOption>
{
    public void Configure(EntityTypeBuilder<MealOption> builder)
    {
        builder.ToTable("meal_options");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Description)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(o => o.SortOrder)
            .IsRequired();

        builder.Property(o => o.ProteinGrams)
            .HasPrecision(8, 2);

        builder.Property(o => o.CarbsGrams)
            .HasPrecision(8, 2);

        builder.Property(o => o.FatGrams)
            .HasPrecision(8, 2);

        builder.HasMany(o => o.Ingredients)
            .WithOne(i => i.MealOption)
            .HasForeignKey(i => i.MealOptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
