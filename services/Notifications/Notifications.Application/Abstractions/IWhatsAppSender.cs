using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.Abstractions;

public interface IWhatsAppSender
{
    Task<Result> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default);
}
