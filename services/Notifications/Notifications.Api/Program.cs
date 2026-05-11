using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notifications.Api.Middleware;
using Notifications.Application;
using Notifications.Infrastructure;
using Notifications.Infrastructure.Persistence;
using Serilog;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

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
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Aplicar migraciones automáticamente al arrancar en desarrollo
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
        await db.Database.MigrateAsync();

        app.MapOpenApi();
    }

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    app.UseStatusCodePages();
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "El servicio de notificaciones terminó inesperadamente.");
}
finally
{
    Log.CloseAndFlush();
}
