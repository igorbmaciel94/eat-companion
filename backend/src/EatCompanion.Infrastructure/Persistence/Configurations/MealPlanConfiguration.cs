using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class MealPlanConfiguration : IEntityTypeConfiguration<MealPlan>
{
    public void Configure(EntityTypeBuilder<MealPlan> builder)
    {
        builder.ToTable("meal_plans");

        builder.HasKey(mp => mp.Id);

        builder.Property(mp => mp.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(mp => mp.SourceFileName)
            .HasMaxLength(200);

        builder.Property(mp => mp.ImportedAt)
            .IsRequired();

        builder.Property(mp => mp.IsActive)
            .IsRequired();

        builder.HasIndex(mp => mp.UserId);

        builder.HasMany(mp => mp.Days)
            .WithOne(d => d.MealPlan)
            .HasForeignKey(d => d.MealPlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
