using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class NoteCategoryConfiguration : IEntityTypeConfiguration<NoteCategory>
{
    public void Configure(EntityTypeBuilder<NoteCategory> builder)
    {
        builder.ToTable("NoteCategories");

        builder.HasKey(nc => new { nc.NoteId, nc.CategoryId });

        // Ordinary FK to Notes.Id - same schema/module, unlike CategoryId which is a plain
        // value with no cross-schema FK (ADR-0003). Cascades so a Note's join rows can't
        // outlive the Note itself.
        builder.HasOne<Note>()
            .WithMany()
            .HasForeignKey(nc => nc.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(nc => nc.CategoryId);
    }
}
