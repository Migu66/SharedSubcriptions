using Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Identity.Api.Extensions;

internal static class MigrationExtensions
{
    /// <summary>
    /// Aplica las migraciones pendientes de EF Core al arrancar la aplicación.
    /// Solo se debe usar en entornos de desarrollo y pruebas.
    /// En producción, las migraciones deben aplicarse como parte del pipeline de despliegue.
    /// </summary>
    internal static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
