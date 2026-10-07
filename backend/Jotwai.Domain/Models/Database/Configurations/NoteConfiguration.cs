using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");

        builder.HasKey(n => n.Id);

        // No FK to identity.Users: Modules don't reference other Modules' schemas (ADR-0003) -
        // UserId is a plain value scoping the row to its owner.
        builder.Property(n => n.UserId)
            .IsRequired();

        builder.HasIndex(n => n.UserId);

        // No FK to organization.Topics either, same reason - plain value validated against
        // Notes' own KnownTopic read model (ADR-0003) before being set. Unique + filtered:
        // a Topic can have at most one active (non-deleted) Note attached (ticket - "1 Topic
        // = continuous document"); soft-deleted Notes and unfiled Notes (TopicId null) are
        // excluded so they never collide with this or each other.
        builder.HasIndex(n => n.TopicId)
            .IsUnique()
            .HasFilter("\"TopicId\" IS NOT NULL AND \"DeletedAt\" IS NULL");

        builder.Property(n => n.Content)
            .IsRequired();

        // Name and parent of the merged Note (ADR-0013). Sibling names are deliberately NOT
        // unique - duplicates are allowed and only warned about, so no index covers Name.
        builder.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasOne<Note>()
            .WithMany()
            .HasForeignKey(n => n.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .IsRequired();
    }
}
