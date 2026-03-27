using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.CalorieTarget)
            .IsRequired();

        builder.Property(u => u.GoalType)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasMany(u => u.MealPlans)
            .WithOne(mp => mp.User)
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.MealLogs)
            .WithOne(ml => ml.User)
            .HasForeignKey(ml => ml.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.WeightEntries)
            .WithOne(we => we.User)
            .HasForeignKey(we => we.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.GroceryLists)
            .WithOne(gl => gl.User)
            .HasForeignKey(gl => gl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
