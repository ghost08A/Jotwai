using Jotwai.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Jotwai.Service;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("identity");

        // Explicit, not ApplyConfigurationsFromAssembly(): Jotwai.Service holds every
        // feature's DbContext in one assembly, so an assembly-wide scan would also pick up
        // other features' IEntityTypeConfiguration<> classes and misapply them here.
        modelBuilder.ApplyConfiguration(new Configurations.UserConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.RefreshTokenConfiguration());
    }
}
