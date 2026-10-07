using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class SearchDocumentConfiguration : IEntityTypeConfiguration<SearchDocument>
{
    public void Configure(EntityTypeBuilder<SearchDocument> builder)
    {
        builder.ToTable("SearchDocuments");

        builder.HasKey(d => d.NoteId);

        // No FK to notes.Notes: Modules don't reference other Modules' schemas (ADR-0003) -
        // NoteId is a plain value identifying the row this projects from.
        builder.Property(d => d.UserId)
            .IsRequired();

        builder.HasIndex(d => d.UserId);

        builder.HasIndex(d => d.TopicId);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Content)
            .IsRequired();

        // Trigram GIN index (ADR-0006) so substring ILIKE matches stay fast as content grows.
        builder.HasIndex(d => d.Content)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .IsRequired();
    }
}
