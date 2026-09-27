using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class TopicSuggestionOutboxEntryConfiguration : IEntityTypeConfiguration<TopicSuggestionOutboxEntry>
{
    public void Configure(EntityTypeBuilder<TopicSuggestionOutboxEntry> builder)
    {
        builder.ToTable("TopicSuggestionOutboxEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TopicId)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        // No HasMaxLength - Note content can be arbitrarily long Markdown; Npgsql maps an
        // unbounded string to `text`.
        builder.Property(e => e.Content);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.Attempts)
            .IsRequired()
            .HasDefaultValue(0);

        // The poller's query ("give me unprocessed rows") is exactly this filter.
        builder.HasIndex(e => e.ProcessedAt);
    }
}
