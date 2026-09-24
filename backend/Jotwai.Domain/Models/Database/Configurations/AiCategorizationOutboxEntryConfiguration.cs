using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class AiCategorizationOutboxEntryConfiguration : IEntityTypeConfiguration<AiCategorizationOutboxEntry>
{
    public void Configure(EntityTypeBuilder<AiCategorizationOutboxEntry> builder)
    {
        builder.ToTable("OutboxEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.NoteId)
            .IsRequired();

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.Attempts)
            .IsRequired()
            .HasDefaultValue(0);

        // The poller's query ("give me unprocessed rows") is exactly this filter.
        builder.HasIndex(e => e.ProcessedAt);
    }
}
