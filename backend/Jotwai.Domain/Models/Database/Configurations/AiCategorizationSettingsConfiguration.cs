using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class AiCategorizationSettingsConfiguration : IEntityTypeConfiguration<AiCategorizationSettings>
{
    // Fixed row: this is a single global setting (ticket 035), not a per-user table.
    public const int SingletonId = 1;
    public const double DefaultConfidenceThreshold = 0.6;

    // Higher bar than Category's (ticket 040) - Merge deletes the new Topic and reassigns its
    // Notes, so a wrong guess is more costly than a wrong Category guess.
    public const double DefaultTopicSuggestionConfidenceThreshold = 0.85;

    public void Configure(EntityTypeBuilder<AiCategorizationSettings> builder)
    {
        builder.ToTable("Settings");

        builder.HasKey(s => s.Id);

        // Always inserted with the fixed SingletonId - not an auto-incrementing identity.
        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.ConfidenceThreshold)
            .IsRequired();

        builder.Property(s => s.TopicSuggestionConfidenceThreshold)
            .IsRequired();

        builder.HasData(new AiCategorizationSettings
        {
            Id = SingletonId,
            ConfidenceThreshold = DefaultConfidenceThreshold,
            TopicSuggestionConfidenceThreshold = DefaultTopicSuggestionConfidenceThreshold,
        });
    }
}
