using Notifications.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Domain.Aggregates;

/// <summary>
/// Proyección local de los datos de contacto de un miembro.
/// Se mantiene actualizada a partir de los eventos de Groups Service.
/// Permite al Notifications Service saber a quién notificar y por qué canales.
/// </summary>
public sealed class NotificationRecipient : Entity<NotificationId>
{
    public string UserId { get; private set; } = string.Empty;
    public GroupId GroupId { get; private init; } = null!;
    public string Email { get; private set; } = string.Empty;
    public string? TelegramChatId { get; private set; }
    public string? WhatsAppPhone { get; private set; }
    public string? FirebaseDeviceToken { get; private set; }

    private NotificationRecipient() { }

    public static NotificationRecipient Create(
        string userId,
        GroupId groupId,
        string email)
    {
        return new NotificationRecipient
        {
            Id = NotificationId.New(),
            UserId = userId,
            GroupId = groupId,
            Email = email
        };
    }

    public void UpdateContactChannels(
        string? telegramChatId,
        string? whatsAppPhone,
        string? firebaseDeviceToken)
    {
        TelegramChatId = telegramChatId;
        WhatsAppPhone = whatsAppPhone;
        FirebaseDeviceToken = firebaseDeviceToken;
    }
}
