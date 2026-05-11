using Analytics.Domain.ReadModels;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence;

public sealed class AnalyticsDbContext : DbContext
{
    public DbSet<GroupSavingsReadModel> GroupSavings => Set<GroupSavingsReadModel>();
    public DbSet<ServiceSpendingReadModel> ServiceSpendings => Set<ServiceSpendingReadModel>();
    public DbSet<DebtHistoryReadModel> DebtHistories => Set<DebtHistoryReadModel>();
    public DbSet<SubscriptionContextReadModel> SubscriptionContexts => Set<SubscriptionContextReadModel>();

    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
