using Groups.Api.Endpoints;
using Groups.Api.Extensions;
using Groups.Api.Middleware;
using Groups.Application;
using Groups.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// Bootstrap logger para capturar errores durante la inicialización,
// antes de que Serilog esté completamente configurado
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog: lee la configuración completa desde appsettings.json
    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    // Capa Application: MediatR + FluentValidation
    builder.Services.AddApplication();

    // Capa Infrastructure: DbContext, repositorios, UnitOfWork, MassTransit + Outbox
    builder.Services.AddInfrastructure(builder.Configuration);

    // Autenticación JWT Bearer
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"]
                    ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Issuer'."),
                ValidAudience = builder.Configuration["Jwt:Audience"]
                    ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Audience'."),
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Secret"]
                            ?? throw new InvalidOperationException("Falta la configuración 'Jwt:Secret'.")))
            };
        });

    builder.Services.AddAuthorization();

    // Manejador global de excepciones (ProblemDetails)
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // OpenAPI
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Aplicar migraciones pendientes al arrancar
    await app.ApplyMigrationsAsync();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSerilogRequestLogging();

    app.UseExceptionHandler();
    app.UseStatusCodePages();

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGroupEndpoints();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "La aplicación terminó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}

