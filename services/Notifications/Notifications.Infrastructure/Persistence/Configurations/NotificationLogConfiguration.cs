using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Aggregates;
using Notifications.Infrastructure.Persistence.Converters;

namespace Notifications.Infrastructure.Persistence.Configurations;

internal sealed class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("NotificationLogs");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasConversion<NotificationIdConverter>()
            .ValueGeneratedNever();

        builder.Property(n => n.RecipientUserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(n => n.Channel)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.SentAt)
            .IsRequired();

        builder.Property(n => n.Success)
            .IsRequired();
    }
}
