using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notifications.Domain.Aggregates;
using Notifications.Infrastructure.Persistence.Converters;

namespace Notifications.Infrastructure.Persistence.Configurations;

internal sealed class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
{
    public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
    {
        builder.ToTable("NotificationRecipients");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion<NotificationIdConverter>()
            .ValueGeneratedNever();

        builder.Property(r => r.UserId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.GroupId)
            .HasConversion<GroupIdConverter>()
            .IsRequired();

        builder.Property(r => r.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(r => r.TelegramChatId)
            .HasMaxLength(100);

        builder.Property(r => r.WhatsAppPhone)
            .HasMaxLength(20);

        builder.Property(r => r.FirebaseDeviceToken)
            .HasMaxLength(500);

        builder.HasIndex(r => new { r.UserId, r.GroupId })
            .IsUnique();
    }
}
