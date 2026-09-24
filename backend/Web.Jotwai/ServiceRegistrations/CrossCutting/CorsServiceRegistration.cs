using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Web.Jotwai.ServiceRegistrations.CrossCutting;

public static class CorsServiceRegistration
{
    public const string PolicyName = "Client";

    /// <summary>
    /// The SPA is deployed as a separate static artifact (ADR-0007), so its origin is
    /// never same-origin with this API - CORS must be explicit. No origins configured
    /// means no cross-origin access allowed, so a deployment that forgets to set this
    /// fails closed rather than open.
    /// </summary>
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy => policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        return services;
    }
}
