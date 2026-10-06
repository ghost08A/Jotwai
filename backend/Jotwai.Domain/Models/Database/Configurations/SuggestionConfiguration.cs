using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class SuggestionConfiguration : IEntityTypeConfiguration<Suggestion>
{
    public void Configure(EntityTypeBuilder<Suggestion> builder)
    {
        builder.ToTable("Suggestions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.NoteId)
            .IsRequired();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        // A Note has at most one *active* Suggestion at a time (CONTEXT.md) - a plain unique
        // index on NoteId would also block ever creating a new one after the first is
        // resolved, so it's filtered to just the unresolved row.
        builder.HasIndex(s => s.NoteId)
            .IsUnique()
            .HasFilter("\"ResolvedAt\" IS NULL");
    }
}
