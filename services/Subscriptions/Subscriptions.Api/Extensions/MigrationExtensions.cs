using Microsoft.EntityFrameworkCore;
using Subscriptions.Infrastructure.Persistence;

namespace Subscriptions.Api.Extensions;

internal static class MigrationExtensions
{
    internal static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubscriptionsDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
