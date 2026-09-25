using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class SearchTopicConfiguration : IEntityTypeConfiguration<SearchTopic>
{
    public void Configure(EntityTypeBuilder<SearchTopic> builder)
    {
        builder.ToTable("SearchTopics");

        builder.HasKey(t => t.TopicId);

        builder.Property(t => t.UserId)
            .IsRequired();

        builder.HasIndex(t => t.UserId);

        builder.Property(t => t.Name)
            .IsRequired();
    }
}
