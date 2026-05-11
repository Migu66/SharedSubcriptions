namespace Notifications.Application.DTOs;

/// <summary>
/// Datos de contacto de un miembro del grupo, proyectados localmente
/// a partir de los eventos de Groups Service.
/// </summary>
public sealed record NotificationRecipientDto(
    string UserId,
    string Email,
    string? TelegramChatId,
    string? WhatsAppPhone,
    string? FirebaseDeviceToken);
