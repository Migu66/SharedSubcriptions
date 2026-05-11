using Microsoft.Extensions.Configuration;
using Notifications.Application.Abstractions;
using SendGrid;
using SendGrid.Helpers.Mail;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Infrastructure.Services;

internal sealed class SendGridEmailSender : IEmailSender
{
    private readonly ISendGridClient _client;
    private readonly EmailAddress _from;

    public SendGridEmailSender(IConfiguration configuration)
    {
        var apiKey = configuration["Notifications:SendGrid:ApiKey"]
            ?? throw new InvalidOperationException("Falta la configuración 'Notifications:SendGrid:ApiKey'.");

        var fromEmail = configuration["Notifications:SendGrid:FromEmail"]
            ?? throw new InvalidOperationException("Falta la configuración 'Notifications:SendGrid:FromEmail'.");

        var fromName = configuration["Notifications:SendGrid:FromName"] ?? "SharedSubscriptions";

        _client = new SendGridClient(apiKey);
        _from = new EmailAddress(fromEmail, fromName);
    }

    public async Task<Result> SendAsync(
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var toAddress = new EmailAddress(to);
        var message = MailHelper.CreateSingleEmail(_from, toAddress, subject, body, body);

        var response = await _client.SendEmailAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.Failure(new Error(
                "Email.SendFailed",
                $"El envío de email falló con el código {(int)response.StatusCode}."));

        return Result.Success();
    }
}
