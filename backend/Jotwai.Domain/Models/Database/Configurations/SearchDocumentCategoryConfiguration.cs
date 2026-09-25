using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class SearchDocumentCategoryConfiguration : IEntityTypeConfiguration<SearchDocumentCategory>
{
    public void Configure(EntityTypeBuilder<SearchDocumentCategory> builder)
    {
        builder.ToTable("SearchDocumentCategories");

        builder.HasKey(dc => new { dc.NoteId, dc.CategoryId });

        // Ordinary FK to SearchDocuments.NoteId - same schema/module, unlike CategoryId
        // which is a plain value with no cross-schema FK (ADR-0003). Cascades so a
        // document's join rows can't outlive it.
        builder.HasOne<SearchDocument>()
            .WithMany()
            .HasForeignKey(dc => dc.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(dc => dc.CategoryId);
    }
}
