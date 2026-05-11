using MassTransit;
using Notifications.Application.Abstractions;
using Notifications.Application.IntegrationEvents;
using Notifications.Domain.ValueObjects;

namespace Notifications.Application.Consumers;

/// <summary>
/// Consume el evento PaymentConfirmedIntegrationEvent publicado por Payments Service.
/// Notifica a cada miembro deudor que el administrador ha confirmado el pago al proveedor
/// y que su parte proporcional está pendiente de reembolso.
/// </summary>
internal sealed class PaymentConfirmedIntegrationEventConsumer
    : IConsumer<PaymentConfirmedIntegrationEvent>
{
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITelegramSender _telegramSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IPushNotificationSender _pushSender;

    public PaymentConfirmedIntegrationEventConsumer(
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

    public async Task Consume(ConsumeContext<PaymentConfirmedIntegrationEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        var recipients = await _recipientRepository.GetByGroupIdAsync(
            GroupId.From(evt.GroupId.Value).Value!,
            cancellationToken);

        if (recipients.Count == 0)
            return;

        var recipientMap = recipients.ToDictionary(r => r.UserId);

        var sendTasks = evt.Quotas
            .Where(q => recipientMap.ContainsKey(q.MemberId.ToString()))
            .SelectMany(q =>
            {
                var recipient = recipientMap[q.MemberId.ToString()];
                var cuota = q.Amount.ToString("F2");
                var mensaje = $"El administrador ha pagado {evt.ServiceName}. " +
                              $"Tu parte es {cuota} {q.Currency}. " +
                              $"Recuerda reembolsarle tu cuota.";

                return BuildSendTasks(recipient, evt.ServiceName, cuota, q.Currency, mensaje, cancellationToken);
            });

        await Task.WhenAll(sendTasks);
    }

    private IEnumerable<Task> BuildSendTasks(
        DTOs.NotificationRecipientDto recipient,
        string serviceName,
        string cuota,
        string currency,
        string mensaje,
        CancellationToken cancellationToken)
    {
        yield return _emailSender.SendAsync(
            to: recipient.Email,
            subject: $"Deuda pendiente: {serviceName}",
            body: mensaje,
            cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(recipient.TelegramChatId))
            yield return _telegramSender.SendAsync(
                chatId: recipient.TelegramChatId,
                message: mensaje,
                cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(recipient.WhatsAppPhone))
            yield return _whatsAppSender.SendAsync(
                phoneNumber: recipient.WhatsAppPhone,
                message: mensaje,
                cancellationToken: cancellationToken);

        if (!string.IsNullOrEmpty(recipient.FirebaseDeviceToken))
            yield return _pushSender.SendAsync(
                deviceToken: recipient.FirebaseDeviceToken,
                title: $"Deuda pendiente: {serviceName}",
                body: $"Tu cuota de {cuota} {currency} está pendiente de pago.",
                cancellationToken: cancellationToken);
    }
}
