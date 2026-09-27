using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class TopicSuggestionConfiguration : IEntityTypeConfiguration<TopicSuggestion>
{
    public void Configure(EntityTypeBuilder<TopicSuggestion> builder)
    {
        builder.ToTable("TopicSuggestions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.TopicId)
            .IsRequired();

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.TargetTopicId)
            .IsRequired();

        builder.Property(s => s.Confidence)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        // A Topic has at most one *active* Topic Suggestion at a time (GLOSSARY.md), mirroring
        // Suggestion's own filtered-unique index.
        builder.HasIndex(s => s.TopicId)
            .IsUnique()
            .HasFilter("\"ResolvedAt\" IS NULL");
    }
}
