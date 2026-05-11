using FluentAssertions;
using MassTransit;
using Notifications.Application.Abstractions;
using Notifications.Application.Consumers;
using Notifications.Application.DTOs;
using Notifications.Application.IntegrationEvents;
using Notifications.Domain.ValueObjects;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.Tests.Consumers;

public class BillingDueSoonIntegrationEventConsumerTests
{
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITelegramSender _telegramSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IPushNotificationSender _pushSender;
    private readonly BillingDueSoonIntegrationEventConsumer _consumer;

    public BillingDueSoonIntegrationEventConsumerTests()
    {
        _recipientRepository = Substitute.For<INotificationRecipientRepository>();
        _emailSender = Substitute.For<IEmailSender>();
        _telegramSender = Substitute.For<ITelegramSender>();
        _whatsAppSender = Substitute.For<IWhatsAppSender>();
        _pushSender = Substitute.For<IPushNotificationSender>();

        _emailSender.SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _telegramSender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _whatsAppSender.SendAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _pushSender.SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _consumer = new BillingDueSoonIntegrationEventConsumer(
            _recipientRepository,
            _emailSender,
            _telegramSender,
            _whatsAppSender,
            _pushSender);
    }

    [Fact]
    public async Task Consume_ConDestinatarios_EnviaEmailATodos()
    {
        // Arrange
        var groupId = GroupId.New();
        var evt = new BillingDueSoonIntegrationEvent(
            groupId: groupId,
            serviceName: "Netflix",
            totalAmount: 9.99m,
            currency: "EUR",
            billingDate: DateTime.UtcNow.AddDays(1));

        var recipients = new List<NotificationRecipientDto>
        {
            new("user1", "user1@test.com", null, null, null),
            new("user2", "user2@test.com", null, null, null)
        };

        _recipientRepository
            .GetByGroupIdAsync(Arg.Any<GroupId>(), Arg.Any<CancellationToken>())
            .Returns(recipients);

        var context = Substitute.For<ConsumeContext<BillingDueSoonIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — debe enviar un email a cada destinatario
        await _emailSender.Received(2).SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_SinDestinatarios_NoEnviaNingunMensaje()
    {
        // Arrange
        var groupId = GroupId.New();
        var evt = new BillingDueSoonIntegrationEvent(
            groupId: groupId,
            serviceName: "Disney+",
            totalAmount: 5.99m,
            currency: "EUR",
            billingDate: DateTime.UtcNow.AddDays(1));

        _recipientRepository
            .GetByGroupIdAsync(Arg.Any<GroupId>(), Arg.Any<CancellationToken>())
            .Returns(new List<NotificationRecipientDto>());

        var context = Substitute.For<ConsumeContext<BillingDueSoonIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — no se debe llamar a ningún canal
        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _telegramSender.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_DestinatarioConTelegram_EnviaAlCanalTelegram()
    {
        // Arrange
        var groupId = GroupId.New();
        var evt = new BillingDueSoonIntegrationEvent(
            groupId: groupId,
            serviceName: "Spotify",
            totalAmount: 4.99m,
            currency: "EUR",
            billingDate: DateTime.UtcNow.AddDays(1));

        var recipients = new List<NotificationRecipientDto>
        {
            new("user1", "user1@test.com", TelegramChatId: "123456789", null, null)
        };

        _recipientRepository
            .GetByGroupIdAsync(Arg.Any<GroupId>(), Arg.Any<CancellationToken>())
            .Returns(recipients);

        var context = Substitute.For<ConsumeContext<BillingDueSoonIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — debe enviar por email Y por Telegram
        await _emailSender.Received(1).SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _telegramSender.Received(1).SendAsync(
            chatId: "123456789", Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
