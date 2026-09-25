using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Domain.Models.Database;

public sealed class SearchDbContext(DbContextOptions<SearchDbContext> options) : DbContext(options)
{
    public DbSet<SearchDocument> SearchDocuments => Set<SearchDocument>();
    public DbSet<SearchDocumentCategory> SearchDocumentCategories => Set<SearchDocumentCategory>();
    public DbSet<SearchTopic> SearchTopics => Set<SearchTopic>();
    public DbSet<SearchCategory> SearchCategories => Set<SearchCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("search");

        // Backs the trigram GIN index (ADR-0006) - Postgres extensions are database-scoped,
        // not schema-scoped, but this is where the Module that needs it declares it.
        modelBuilder.HasPostgresExtension("pg_trgm");

        // Explicit, not ApplyConfigurationsFromAssembly(): Jotwai.Service holds every
        // feature's DbContext in one assembly, so an assembly-wide scan would also pick up
        // other features' IEntityTypeConfiguration<> classes and misapply them here.
        modelBuilder.ApplyConfiguration(new Configurations.SearchDocumentConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SearchDocumentCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SearchTopicConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.SearchCategoryConfiguration());
    }
}
