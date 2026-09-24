using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jotwai.Domain.Models.Database;

/// <summary>
/// Design-time factory so `dotnet ef migrations add` can build an AiCategorizationDbContext
/// without needing Web.Jotwai's runtime configuration/DI available.
/// </summary>
public sealed class AiCategorizationDbContextFactory : IDesignTimeDbContextFactory<AiCategorizationDbContext>
{
    public AiCategorizationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AiCategorizationDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=jotwai;Username=jotwai;Password=jotwai",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ai_categorization"));

        return new AiCategorizationDbContext(optionsBuilder.Options);
    }
}
