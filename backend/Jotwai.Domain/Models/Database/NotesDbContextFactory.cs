using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Jotwai.Domain.Models.Database;

/// <summary>
/// Design-time factory so `dotnet ef migrations add` can build a NotesDbContext
/// without needing Web.Jotwai's runtime configuration/DI available.
/// </summary>
public sealed class NotesDbContextFactory : IDesignTimeDbContextFactory<NotesDbContext>
{
    public NotesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotesDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=jotwai;Username=jotwai;Password=jotwai",
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "notes"));

        return new NotesDbContext(optionsBuilder.Options);
    }
}
