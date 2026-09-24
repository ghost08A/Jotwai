using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service;

public sealed class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options) : DbContext(options)
{
    public DbSet<Topic> Topics => Set<Topic>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("organization");

        // Explicit, not ApplyConfigurationsFromAssembly(): Jotwai.Service holds every
        // feature's DbContext in one assembly, so an assembly-wide scan would also pick up
        // other features' IEntityTypeConfiguration<> classes and misapply them here.
        modelBuilder.ApplyConfiguration(new Configurations.TopicConfiguration());
    }
}
