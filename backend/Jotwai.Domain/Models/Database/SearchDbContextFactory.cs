using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jotwai.Domain.Models.Database;

/// <summary>
/// Design-time factory so `dotnet ef migrations add` can build a SearchDbContext
/// without needing Web.Jotwai's runtime configuration/DI available.
/// </summary>
public sealed class SearchDbContextFactory : IDesignTimeDbContextFactory<SearchDbContext>
{
    public SearchDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SearchDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=jotwai;Username=jotwai;Password=jotwai",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "search"));

        return new SearchDbContext(optionsBuilder.Options);
    }
}
