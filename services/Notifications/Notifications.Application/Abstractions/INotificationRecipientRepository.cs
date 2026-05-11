using Notifications.Application.DTOs;
using Notifications.Domain.ValueObjects;

namespace Notifications.Application.Abstractions;

/// <summary>
/// Accede a la proyección local de destinatarios que el Notifications Service
/// mantiene a partir de los eventos de Groups Service.
/// </summary>
public interface INotificationRecipientRepository
{
    Task<IReadOnlyList<NotificationRecipientDto>> GetByGroupIdAsync(
        GroupId groupId,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(
        NotificationRecipientDto recipient,
        GroupId groupId,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string userId,
        GroupId groupId,
        CancellationToken cancellationToken = default);
}
