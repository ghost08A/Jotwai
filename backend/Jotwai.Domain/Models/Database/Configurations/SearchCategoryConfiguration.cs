using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class SearchCategoryConfiguration : IEntityTypeConfiguration<SearchCategory>
{
    public void Configure(EntityTypeBuilder<SearchCategory> builder)
    {
        builder.ToTable("SearchCategories");

        builder.HasKey(c => c.CategoryId);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.HasIndex(c => c.UserId);

        builder.Property(c => c.Name)
            .IsRequired();
    }
}
