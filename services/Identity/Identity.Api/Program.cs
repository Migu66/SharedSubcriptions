using Identity.Api.Endpoints;
using Identity.Api.Extensions;
using Identity.Api.Middleware;
using Identity.Application;
using Identity.Infrastructure;
using Serilog;

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

    // Capa Infrastructure: DbContext, repositorios, UnitOfWork, JWT, MassTransit + Outbox
    builder.Services.AddInfrastructure(builder.Configuration);

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

    app.MapIdentityEndpoints();

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
