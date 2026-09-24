using Jotwai.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

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

builder.Services.AddDomainEventing();
builder.Services.AddIdentityFeature(builder.Configuration);
builder.Services.AddOrganizationFeature(builder.Configuration);
builder.Services.AddNotesFeature(builder.Configuration);
builder.Services.AddCustomCors(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<IdentityDbContext>()
    .AddDbContextCheck<OrganizationDbContext>()
    .AddDbContextCheck<NotesDbContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<OrganizationDbContext>().Database.Migrate();
    scope.ServiceProvider.GetRequiredService<NotesDbContext>().Database.Migrate();
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

// Exposed for WebApplicationFactory<Program> in integration tests.
public partial class Program;
