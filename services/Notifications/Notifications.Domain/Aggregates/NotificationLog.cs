using Notifications.Domain.Enums;
using Notifications.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Domain.Aggregates;

public sealed class NotificationLog : Entity<NotificationId>
{
    public string RecipientUserId { get; private init; } = string.Empty;
    public NotificationChannel Channel { get; private init; }
    public string Message { get; private init; } = string.Empty;
    public DateTime SentAt { get; private init; }
    public bool Success { get; private init; }

    private NotificationLog() { }

    public static NotificationLog Create(
        string recipientUserId,
        NotificationChannel channel,
        string message,
        DateTime sentAt,
        bool success)
    {
        return new NotificationLog
        {
            Id = NotificationId.New(),
            RecipientUserId = recipientUserId,
            Channel = channel,
            Message = message,
            SentAt = sentAt,
            Success = success
        };
    }
}
