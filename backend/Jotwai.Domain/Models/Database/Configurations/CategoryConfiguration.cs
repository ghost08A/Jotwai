using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        // Category names are unique system-wide per user, case-insensitively (CONTEXT.md) -
        // flat, so a single index suffices (no ParentId nullability pitfall like Topic's).
        builder.HasIndex(c => new { c.UserId, c.NormalizedName })
            .IsUnique();

        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .IsRequired();
    }
}
