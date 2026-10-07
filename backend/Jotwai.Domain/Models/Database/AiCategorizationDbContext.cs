using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Domain.Models.Database;

public sealed class AiCategorizationDbContext(DbContextOptions<AiCategorizationDbContext> options) : DbContext(options)
{
    public DbSet<AiCategorizationOutboxEntry> OutboxEntries => Set<AiCategorizationOutboxEntry>();
    public DbSet<Suggestion> Suggestions => Set<Suggestion>();
    public DbSet<SuggestionCandidate> SuggestionCandidates => Set<SuggestionCandidate>();
    public DbSet<AiCategorizationSettings> Settings => Set<AiCategorizationSettings>();
    public DbSet<CategoryProjection> CategoryProjections => Set<CategoryProjection>();
    public DbSet<TopicProjection> TopicProjections => Set<TopicProjection>();
    public DbSet<TopicSuggestion> TopicSuggestions => Set<TopicSuggestion>();
    public DbSet<TopicSuggestionOutboxEntry> TopicSuggestionOutboxEntries => Set<TopicSuggestionOutboxEntry>();
    public DbSet<NoteProjection> NoteProjections => Set<NoteProjection>();
    public DbSet<NoteSuggestion> NoteSuggestions => Set<NoteSuggestion>();
    public DbSet<NoteSuggestionOutboxEntry> NoteSuggestionOutboxEntries => Set<NoteSuggestionOutboxEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ai_categorization");

        // Explicit, not ApplyConfigurationsFromAssembly(): Jotwai.Service holds every
        // feature's DbContext in one assembly, so an assembly-wide scan would also pick up
        // other features' IEntityTypeConfiguration<> classes and misapply them here.
        modelBuilder.ApplyConfiguration(new Configurations.AiCategorizationOutboxEntryConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SuggestionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SuggestionCandidateConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.AiCategorizationSettingsConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.CategoryProjectionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TopicProjectionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TopicSuggestionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.TopicSuggestionOutboxEntryConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NoteProjectionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NoteSuggestionConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NoteSuggestionOutboxEntryConfiguration());
    }
}
