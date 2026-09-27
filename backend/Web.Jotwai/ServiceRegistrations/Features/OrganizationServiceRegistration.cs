using Jotwai.Domain.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Jotwai.ServiceRegistrations.Features;

public static class OrganizationServiceRegistration
{
    /// <summary>
    /// Registers the Organization feature. This is the only thing Web.Jotwai needs to
    /// call to wire up this feature - everything else stays internal to it.
    /// </summary>
    public static IServiceCollection AddOrganizationFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");

        services.AddDbContext<OrganizationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "organization")));

        // ITopicsService/TopicsService and ICategoriesService/CategoriesService wired by
        // RegisterAutoServices (Web.Jotwai/Program.cs).

        return services;
    }
}
