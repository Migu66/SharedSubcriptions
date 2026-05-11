using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.Abstractions;

public interface IEmailSender
{
    Task<Result> SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
