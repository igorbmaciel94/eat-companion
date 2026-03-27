using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class GroceryListConfiguration : IEntityTypeConfiguration<GroceryList>
{
    public void Configure(EntityTypeBuilder<GroceryList> builder)
    {
        builder.ToTable("grocery_lists");

        builder.HasKey(gl => gl.Id);

        builder.Property(gl => gl.StartDate)
            .IsRequired();

        builder.Property(gl => gl.EndDate)
            .IsRequired();

        builder.Property(gl => gl.CreatedAt)
            .IsRequired();

        builder.HasOne(gl => gl.User)
            .WithMany(u => u.GroceryLists)
            .HasForeignKey(gl => gl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(gl => gl.MealPlan)
            .WithMany(mp => mp.GroceryLists)
            .HasForeignKey(gl => gl.MealPlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(gl => gl.Items)
            .WithOne(gi => gi.GroceryList)
            .HasForeignKey(gi => gi.GroceryListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
