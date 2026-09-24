using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class KnownTopicConfiguration : IEntityTypeConfiguration<KnownTopic>
{
    public void Configure(EntityTypeBuilder<KnownTopic> builder)
    {
        builder.ToTable("KnownTopics");

        builder.HasKey(t => t.TopicId);

        builder.Property(t => t.UserId)
            .IsRequired();
    }
}
