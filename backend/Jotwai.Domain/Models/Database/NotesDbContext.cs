using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Domain.Models.Database;

public sealed class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<KnownTopic> KnownTopics => Set<KnownTopic>();
    public DbSet<KnownCategory> KnownCategories => Set<KnownCategory>();
    public DbSet<NoteCategory> NoteCategories => Set<NoteCategory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notes");

        // Explicit, not ApplyConfigurationsFromAssembly(): Jotwai.Service holds every
        // feature's DbContext in one assembly, so an assembly-wide scan would also pick up
        // other features' IEntityTypeConfiguration<> classes and misapply them here.
        modelBuilder.ApplyConfiguration(new Configurations.NoteConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.KnownTopicConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.KnownCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.NoteCategoryConfiguration());
    }
}
