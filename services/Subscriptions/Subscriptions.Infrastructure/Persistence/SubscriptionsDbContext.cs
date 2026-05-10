using MassTransit;
using Microsoft.EntityFrameworkCore;
using Subscriptions.Domain.Aggregates;

namespace Subscriptions.Infrastructure.Persistence;

public sealed class SubscriptionsDbContext : DbContext
{
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public SubscriptionsDbContext(DbContextOptions<SubscriptionsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubscriptionsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
