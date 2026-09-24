using System.Text;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models;
using Jotwai.Domain.Models.Database;
using Jotwai.Service;
using Jotwai.Service.ImplementServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Web.Jotwai.ServiceRegistrations.Features;

public static class IdentityServiceRegistration
{
    /// <summary>
    /// Registers the Identity feature. This is the only thing Web.Jotwai needs to
    /// call to wire up this feature - everything else stays internal to it.
    /// </summary>
    public static IServiceCollection AddIdentityFeature(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");

        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "identity")));

        var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("Missing 'Jwt' configuration section.");

        // Bound once here, not also via services.Configure<JwtOptions>(...): that would be
        // a second, independent binding of the same section for the same IOptions<JwtOptions>
        // consumers (TokenService, tests) to stay in sync with.
        services.AddSingleton(Options.Create(jwtOptions));
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

        // ITokenService/TokenService and IAuthService/AuthService wired by
        // RegisterAutoServices (Web.Jotwai/Program.cs).

        // The Identity Module's public auth contract (see AuthController.Me), wired once
        // here for the whole API.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep claim types as issued ("sub", not the default-remapped
                // ClaimTypes.NameIdentifier) so callers can read JwtRegisteredClaimNames.Sub
                // directly off User, matching what TokenService actually puts in the token.
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                };
            });
        services.AddAuthorization();

        return services;
    }
}
