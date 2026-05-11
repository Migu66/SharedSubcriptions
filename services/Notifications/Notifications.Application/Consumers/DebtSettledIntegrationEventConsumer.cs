using MassTransit;
using Notifications.Application.Abstractions;
using Notifications.Application.IntegrationEvents;

namespace Notifications.Application.Consumers;

/// <summary>
/// Consume el evento DebtSettledIntegrationEvent publicado por Payments Service.
/// Notifica al administrador (acreedor) que un miembro ha saldado su deuda,
/// indicando el importe recibido.
/// </summary>
internal sealed class DebtSettledIntegrationEventConsumer
    : IConsumer<DebtSettledIntegrationEvent>
{
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITelegramSender _telegramSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IPushNotificationSender _pushSender;

    public DebtSettledIntegrationEventConsumer(
        INotificationRecipientRepository recipientRepository,
        IEmailSender emailSender,
        ITelegramSender telegramSender,
        IWhatsAppSender whatsAppSender,
        IPushNotificationSender pushSender)
    {
        _recipientRepository = recipientRepository;
        _emailSender = emailSender;
        _telegramSender = telegramSender;
        _whatsAppSender = whatsAppSender;
        _pushSender = pushSender;
    }

    public async Task Consume(ConsumeContext<DebtSettledIntegrationEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        var creditor = await _recipientRepository.GetByUserIdAsync(
            evt.CreditorId.ToString(),
            cancellationToken);

        if (creditor is null)
            return;

        var importe = evt.Amount.ToString("F2");
        var mensaje = $"Un miembro ha saldado su deuda. " +
                      $"Has recibido {importe} {evt.Currency}.";

        var sendTasks = BuildSendTasks(creditor, importe, evt.Currency, mensaje, cancellationToken);

        await Task.WhenAll(sendTasks);
    }

    private IEnumerable<Task> BuildSendTasks(
        DTOs.NotificationRecipientDto creditor,
        string importe,
        string currency,
        string mensaje,
        CancellationToken cancellationToken)
    {
        yield return _emailSender.SendAsync(
            to: creditor.Email,
            subject: "Deuda saldada",
            body: mensaje,
            cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(creditor.TelegramChatId))
            yield return _telegramSender.SendAsync(
                chatId: creditor.TelegramChatId,
                message: mensaje,
                cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(creditor.WhatsAppPhone))
            yield return _whatsAppSender.SendAsync(
                phoneNumber: creditor.WhatsAppPhone,
                message: mensaje,
                cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(creditor.FirebaseDeviceToken))
            yield return _pushSender.SendAsync(
                deviceToken: creditor.FirebaseDeviceToken,
                title: "Deuda saldada",
                body: $"Has recibido {importe} {currency}.",
                cancellationToken: cancellationToken);
    }
}
