using MassTransit;
using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Aggregates;

namespace Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext : DbContext
{
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<NotificationRecipient> NotificationRecipients => Set<NotificationRecipient>();

    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationsDbContext).Assembly);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
    }
}
