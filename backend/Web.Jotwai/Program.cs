using System.Reflection;
using Jotwai.Domain.Interfaces;
using Jotwai.Domain.Models.Database;
using Jotwai.Service.ImplementServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Web.Jotwai.ServiceRegistrations.CrossCutting;
using Web.Jotwai.ServiceRegistrations.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(options =>
{
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste an access token from /api/auth/login or /api/auth/register.",
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, bearerScheme);
    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme)] = [],
    });
});

RegisterAutoServices(builder);
builder.Services.AddDomainEventing();
builder.Services.AddIdentityFeature(builder.Configuration);
builder.Services.AddOrganizationFeature(builder.Configuration);
builder.Services.AddNotesFeature(builder.Configuration);
builder.Services.AddAiCategorizationFeature(builder.Configuration);
builder.Services.AddSearchFeature(builder.Configuration);
builder.Services.AddCustomCors(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>()
    .AddDbContextCheck<OrganizationDbContext>()
    .AddDbContextCheck<NotesDbContext>()
    .AddDbContextCheck<AiCategorizationDbContext>()
    .AddDbContextCheck<SearchDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<OrganizationDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<NotesDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<AiCategorizationDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<SearchDbContext>().Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// No UseHttpsRedirection: TLS is terminated upstream (reverse proxy/hosting), not by
// Kestrel — matching the company's standard convention. Redirecting here would also
// break CORS preflight (a preflight response can't be a redirect), which is exactly
// what broke local dev against the SPA on a plain-http origin.
app.UseRouting();
app.UseCors(CorsServiceRegistration.PolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Reflection-based DI wiring for the "I<Name>Service" / "<Name>Service" pairing (see
// backend/CLAUDE.md Naming - "Auto-DI" convention). Every interface in
// Jotwai.Domain/Interfaces/ named IXxxService is matched by name to a class in
// Jotwai.Service/ImplementServices/ named XxxService and registered Scoped, so
// <Module>ServiceRegistration files don't each carry their own AddScoped line for it.
//
// Anything that isn't that exact 1:1 pairing - DbContext, IEventHandler<T>,
// HttpClient-backed providers like IAiProvider, singletons - stays a manual
// AddScoped/AddSingleton call in its module's registration file, same as before.
static void RegisterAutoServices(WebApplicationBuilder builder)
{
    var interfaceAssembly = Assembly.GetAssembly(typeof(IAuthService))!.GetTypes().Where(x => x.Name.EndsWith("Service"));
    var implementationAssembly = Assembly.GetAssembly(typeof(AuthService))!.GetTypes().Where(x => x.Name.EndsWith("Service"));
    foreach (var @interface in interfaceAssembly)
    {
        var interfaceName = @interface.Name;
        var implement = implementationAssembly.FirstOrDefault(c => c.IsClass && interfaceName[1..] == c.Name);
        if (implement != null)
            builder.Services.AddScoped(@interface, implement);
    }
}

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;
