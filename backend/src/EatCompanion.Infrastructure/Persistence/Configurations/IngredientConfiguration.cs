using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EatCompanion.Infrastructure.Persistence.Configurations;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> builder)
    {
        builder.ToTable("ingredients");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.NamePt)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Amount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(i => i.Unit)
            .IsRequired();

        builder.Property(i => i.Category)
            .IsRequired();
    }
}
