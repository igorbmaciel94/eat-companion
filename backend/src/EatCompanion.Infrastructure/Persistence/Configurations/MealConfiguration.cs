using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class MealConfiguration : IEntityTypeConfiguration<Meal>
{
    public void Configure(EntityTypeBuilder<Meal> builder)
    {
        builder.ToTable("meals");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MealType)
            .IsRequired();

        builder.Property(m => m.SortOrder)
            .IsRequired();

        builder.HasMany(m => m.Options)
            .WithOne(o => o.Meal)
            .HasForeignKey(o => o.MealId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
