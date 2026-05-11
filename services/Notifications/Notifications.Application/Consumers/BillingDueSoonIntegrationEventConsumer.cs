using MassTransit;
using Notifications.Application.Abstractions;
using Notifications.Application.IntegrationEvents;
using Notifications.Domain.ValueObjects;

namespace Notifications.Application.Consumers;

/// <summary>
/// Consume el evento BillingDueSoonIntegrationEvent publicado por Subscriptions Service.
/// Envía recordatorios personalizados a todos los miembros del grupo indicándoles
/// que su cuota está pendiente de pago antes de la fecha de renovación.
/// </summary>
internal sealed class BillingDueSoonIntegrationEventConsumer
    : IConsumer<BillingDueSoonIntegrationEvent>
{
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITelegramSender _telegramSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IPushNotificationSender _pushSender;

    public BillingDueSoonIntegrationEventConsumer(
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

    public async Task Consume(ConsumeContext<BillingDueSoonIntegrationEvent> context)
    {
        var evt = context.Message;
        var cancellationToken = context.CancellationToken;

        var recipients = await _recipientRepository.GetByGroupIdAsync(
            GroupId.From(evt.GroupId.Value).Value!,
            cancellationToken);

        if (recipients.Count == 0)
            return;

        var billingDate = evt.BillingDate.ToString("dd/MM/yyyy");
        var cuota = (evt.TotalAmount / recipients.Count).ToString("F2");
        var mensaje = $"Hola. Mañana se renueva {evt.ServiceName}. " +
                      $"Tu cuota de {cuota} {evt.Currency} está pendiente. " +
                      $"Fecha de cobro: {billingDate}.";

        var sendTasks = recipients.SelectMany(recipient => BuildSendTasks(
            recipient, evt.ServiceName, cuota, evt.Currency, billingDate, mensaje, cancellationToken));

        await Task.WhenAll(sendTasks);
    }

    private IEnumerable<Task> BuildSendTasks(
        DTOs.NotificationRecipientDto recipient,
        string serviceName,
        string cuota,
        string currency,
        string billingDate,
        string mensaje,
        CancellationToken cancellationToken)
    {
        yield return _emailSender.SendAsync(
            to: recipient.Email,
            subject: $"Recordatorio de pago: {serviceName}",
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
                title: $"Pago pendiente: {serviceName}",
                body: $"Tu cuota de {cuota} {currency} vence el {billingDate}.",
                cancellationToken: cancellationToken);
    }
}
