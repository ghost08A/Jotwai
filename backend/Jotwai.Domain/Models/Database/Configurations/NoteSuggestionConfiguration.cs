using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class NoteSuggestionConfiguration : IEntityTypeConfiguration<NoteSuggestion>
{
    public void Configure(EntityTypeBuilder<NoteSuggestion> builder)
    {
        builder.ToTable("NoteSuggestions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.NoteId)
            .IsRequired();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.TargetNoteId)
            .IsRequired();

        builder.Property(s => s.Confidence)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        // A Note has at most one *active* Note Suggestion at a time (CONTEXT.md).
        builder.HasIndex(s => s.NoteId)
            .IsUnique()
            .HasFilter("\"ResolvedAt\" IS NULL");
    }
}
