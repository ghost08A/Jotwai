using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Service.Configurations;

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
        // Notes' own KnownTopic read model (ADR-0003) before being set.
        builder.HasIndex(n => n.TopicId);

        builder.Property(n => n.Content)
            .IsRequired();

        builder.Property(n => n.CreatedAt)
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .IsRequired();
    }
}
