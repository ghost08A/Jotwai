using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class NoteProjectionConfiguration : IEntityTypeConfiguration<NoteProjection>
{
    public void Configure(EntityTypeBuilder<NoteProjection> builder)
    {
        builder.ToTable("NoteProjections");

        builder.HasKey(n => n.NoteId);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(n => n.UserId);
    }
}
