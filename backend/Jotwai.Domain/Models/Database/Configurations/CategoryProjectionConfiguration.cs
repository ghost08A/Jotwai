using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class CategoryProjectionConfiguration : IEntityTypeConfiguration<CategoryProjection>
{
    public void Configure(EntityTypeBuilder<CategoryProjection> builder)
    {
        builder.ToTable("CategoryProjections");

        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.UserId);
    }
}
