using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.Abstractions;

public interface ITelegramSender
{
    Task<Result> SendAsync(
        string chatId,
        string message,
        CancellationToken cancellationToken = default);
}
