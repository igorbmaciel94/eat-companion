using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class MealPlanDayConfiguration : IEntityTypeConfiguration<MealPlanDay>
{
    public void Configure(EntityTypeBuilder<MealPlanDay> builder)
    {
        builder.ToTable("meal_plan_days");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DayOfWeek)
            .IsRequired();

        builder.HasIndex(d => new { d.MealPlanId, d.DayOfWeek });

        builder.HasMany(d => d.Meals)
            .WithOne(m => m.MealPlanDay)
            .HasForeignKey(m => m.MealPlanDayId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
