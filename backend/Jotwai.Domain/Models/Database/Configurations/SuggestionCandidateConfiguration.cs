using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Jotwai.Domain.Models.Database.Configurations;

public sealed class SuggestionCandidateConfiguration : IEntityTypeConfiguration<SuggestionCandidate>
{
    public void Configure(EntityTypeBuilder<SuggestionCandidate> builder)
    {
        builder.ToTable("SuggestionCandidates");

        builder.HasKey(c => c.Id);

        // Ordinary FK - Suggestion is owned by this same schema/module, unlike CategoryId
        // which is a plain value with no cross-schema FK (ADR-0003, ticket 034).
        builder.HasOne<Suggestion>()
            .WithMany()
            .HasForeignKey(c => c.SuggestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.NewCategoryName)
            .HasMaxLength(200);

        builder.Property(c => c.Rank)
            .IsRequired();

        builder.Property(c => c.Confidence)
            .IsRequired();
    }
}
