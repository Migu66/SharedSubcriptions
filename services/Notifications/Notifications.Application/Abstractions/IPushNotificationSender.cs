using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.Abstractions;

public interface IPushNotificationSender
{
    Task<Result> SendAsync(
        string deviceToken,
        string title,
        string body,
        CancellationToken cancellationToken = default);
}
