using Jotwai.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace Jotwai.IntegrationTests;

/// <summary>
/// Boots the real Web.Jotwai host against a throwaway Postgres container, so tests
/// exercise the public HTTP API only - never the feature's internals directly.
///
/// Note: overriding IdentityDbContext's registration in ConfigureServices (not via
/// ConfigureAppConfiguration) is deliberate - Program.cs reads the connection string
/// eagerly to build DbContextOptions, before a config-only override would be visible.
/// ConfigureServices runs after Program.cs's own registration, so we replace it here.
/// </summary>
public sealed class JotwaiApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("jotwai")
        .WithUsername("jotwai")
        .WithPassword("jotwai")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<IdentityDbContext>>();
            services.AddDbContext<IdentityDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

            services.RemoveAll<DbContextOptions<OrganizationDbContext>>();
            services.AddDbContext<OrganizationDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "organization")));

            services.RemoveAll<DbContextOptions<NotesDbContext>>();
            services.AddDbContext<NotesDbContext>(options =>
                options.UseNpgsql(_postgres.GetConnectionString(),
                    npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "notes")));
        });
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.StopAsync();
        await base.DisposeAsync();
    }
}
