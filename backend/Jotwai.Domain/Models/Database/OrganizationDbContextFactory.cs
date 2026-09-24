using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jotwai.Domain.Models.Database;

/// <summary>
/// Design-time factory so `dotnet ef migrations add` can build an OrganizationDbContext
/// without needing Web.Jotwai's runtime configuration/DI available.
/// </summary>
public sealed class OrganizationDbContextFactory : IDesignTimeDbContextFactory<OrganizationDbContext>
{
    public OrganizationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrganizationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=jotwai;Username=jotwai;Password=jotwai",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "organization"));

        return new OrganizationDbContext(optionsBuilder.Options);
    }
}
