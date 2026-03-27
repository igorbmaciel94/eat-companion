using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class GroceryItemConfiguration : IEntityTypeConfiguration<GroceryItem>
{
    public void Configure(EntityTypeBuilder<GroceryItem> builder)
    {
        builder.ToTable("grocery_items");

        builder.HasKey(gi => gi.Id);

        builder.Property(gi => gi.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(gi => gi.TotalAmount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(gi => gi.Unit)
            .IsRequired();

        builder.Property(gi => gi.Category)
            .IsRequired();
    }
}
