using Microsoft.EntityFrameworkCore;
using Notifications.Application.Abstractions;
using Notifications.Application.DTOs;
using Notifications.Domain.Aggregates;
using Notifications.Domain.ValueObjects;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRecipientRepository : INotificationRecipientRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationRecipientRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<NotificationRecipientDto>> GetByGroupIdAsync(
        GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return await _context.NotificationRecipients
            .Where(r => r.GroupId == groupId)
            .Select(r => new NotificationRecipientDto(
                r.UserId,
                r.Email,
                r.TelegramChatId,
                r.WhatsAppPhone,
                r.FirebaseDeviceToken))
            .ToListAsync(cancellationToken);
    }

    public async Task<NotificationRecipientDto?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var recipient = await _context.NotificationRecipients
            .Where(r => r.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (recipient is null)
            return null;

        return new NotificationRecipientDto(
            recipient.UserId,
            recipient.Email,
            recipient.TelegramChatId,
            recipient.WhatsAppPhone,
            recipient.FirebaseDeviceToken);
    }

    public async Task UpsertAsync(
        NotificationRecipientDto dto,
        GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.NotificationRecipients
            .FirstOrDefaultAsync(
                r => r.UserId == dto.UserId && r.GroupId == groupId,
                cancellationToken);

        if (existing is not null)
        {
            existing.UpdateContactChannels(
                dto.TelegramChatId,
                dto.WhatsAppPhone,
                dto.FirebaseDeviceToken);
        }
        else
        {
            var recipient = NotificationRecipient.Create(dto.UserId, groupId, dto.Email);
            recipient.UpdateContactChannels(
                dto.TelegramChatId,
                dto.WhatsAppPhone,
                dto.FirebaseDeviceToken);

            await _context.NotificationRecipients.AddAsync(recipient, cancellationToken);
        }
    }

    public async Task RemoveAsync(
        string userId,
        GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        var recipient = await _context.NotificationRecipients
            .FirstOrDefaultAsync(
                r => r.UserId == userId && r.GroupId == groupId,
                cancellationToken);

        if (recipient is not null)
            _context.NotificationRecipients.Remove(recipient);
    }
}
