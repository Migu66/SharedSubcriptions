using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Subscriptions.Infrastructure.Persistence;

internal sealed class SubscriptionsDbContextFactory
    : IDesignTimeDbContextFactory<SubscriptionsDbContext>
{
    public SubscriptionsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SubscriptionsDbContext>()
            .UseSqlServer("Server=localhost;Database=SubscriptionsDb;Trusted_Connection=True;")
            .Options;

        return new SubscriptionsDbContext(options);
    }
}
