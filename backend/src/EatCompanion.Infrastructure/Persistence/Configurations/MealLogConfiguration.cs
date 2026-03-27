using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class MealLogConfiguration : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.ToTable("meal_logs");

        builder.HasKey(ml => ml.Id);

        builder.Property(ml => ml.Date)
            .IsRequired();

        builder.Property(ml => ml.MealType)
            .IsRequired();

        builder.Property(ml => ml.Status)
            .IsRequired();

        builder.Property(ml => ml.Notes)
            .HasColumnType("text");

        builder.Property(ml => ml.CreatedAt)
            .IsRequired();

        builder.HasIndex(ml => new { ml.UserId, ml.Date });

        builder.HasOne(ml => ml.MealOption)
            .WithMany()
            .HasForeignKey(ml => ml.MealOptionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
