using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId)
            .IsRequired();

        // Adjacency list (ticket 029) - self-FK within the same schema/module, so this is
        // an ordinary FK, not the cross-module reference Note.TopicId is.
        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.NormalizedName)
            .IsRequired()
            .HasMaxLength(200);

        // Sibling names must be unique case-insensitively per parent (GLOSSARY.md) - split
        // into two filtered indexes because a plain composite unique index would treat every
        // NULL ParentId (root Topics) as distinct, silently allowing duplicate root names.
        builder.HasIndex(t => new { t.UserId, t.ParentId, t.NormalizedName })
            .IsUnique()
            .HasFilter("\"ParentId\" IS NOT NULL");

        builder.HasIndex(t => new { t.UserId, t.NormalizedName })
            .IsUnique()
            .HasFilter("\"ParentId\" IS NULL");

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .IsRequired();
    }
}
