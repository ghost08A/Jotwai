using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class TopicProjectionConfiguration : IEntityTypeConfiguration<TopicProjection>
{
    public void Configure(EntityTypeBuilder<TopicProjection> builder)
    {
        builder.ToTable("TopicProjections");

        builder.HasKey(t => t.TopicId);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(t => t.UserId);
    }
}
