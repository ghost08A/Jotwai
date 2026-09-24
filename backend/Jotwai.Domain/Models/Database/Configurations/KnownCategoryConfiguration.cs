using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class KnownCategoryConfiguration : IEntityTypeConfiguration<KnownCategory>
{
    public void Configure(EntityTypeBuilder<KnownCategory> builder)
    {
        builder.ToTable("KnownCategories");

        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.UserId)
            .IsRequired();
    }
}
