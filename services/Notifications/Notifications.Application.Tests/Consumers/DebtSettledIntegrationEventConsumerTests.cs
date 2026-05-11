using FluentAssertions;
using MassTransit;
using Notifications.Application.Abstractions;
using Notifications.Application.Consumers;
using Notifications.Application.DTOs;
using Notifications.Application.IntegrationEvents;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Application.Tests.Consumers;

public class DebtSettledIntegrationEventConsumerTests
{
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITelegramSender _telegramSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IPushNotificationSender _pushSender;
    private readonly DebtSettledIntegrationEventConsumer _consumer;

    public DebtSettledIntegrationEventConsumerTests()
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

        _consumer = new DebtSettledIntegrationEventConsumer(
            _recipientRepository,
            _emailSender,
            _telegramSender,
            _whatsAppSender,
            _pushSender);
    }

    [Fact]
    public async Task Consume_AcreedorExiste_EnviaEmailAlAcreedor()
    {
        // Arrange
        var creditorId = Guid.NewGuid();
        var debtorId = Guid.NewGuid();

        var evt = new DebtSettledIntegrationEvent(
            debtorId: debtorId,
            creditorId: creditorId,
            amount: 3.33m,
            currency: "EUR");

        var creditor = new NotificationRecipientDto(
            creditorId.ToString(),
            "admin@test.com",
            TelegramChatId: null,
            WhatsAppPhone: null,
            FirebaseDeviceToken: null);

        _recipientRepository
            .GetByUserIdAsync(creditorId.ToString(), Arg.Any<CancellationToken>())
            .Returns(creditor);

        var context = Substitute.For<ConsumeContext<DebtSettledIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — solo se notifica al acreedor por email
        await _emailSender.Received(1).SendAsync(
            to: "admin@test.com",
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_AcreedorNoExiste_NoEnviaNingunMensaje()
    {
        // Arrange
        var evt = new DebtSettledIntegrationEvent(
            debtorId: Guid.NewGuid(),
            creditorId: Guid.NewGuid(),
            amount: 5.00m,
            currency: "EUR");

        _recipientRepository
            .GetByUserIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((NotificationRecipientDto?)null);

        var context = Substitute.For<ConsumeContext<DebtSettledIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — no se conoce al acreedor, no se envía nada
        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_AcreedorConTodosLosCanales_EnviaATodosLosCanales()
    {
        // Arrange
        var creditorId = Guid.NewGuid();

        var evt = new DebtSettledIntegrationEvent(
            debtorId: Guid.NewGuid(),
            creditorId: creditorId,
            amount: 2.50m,
            currency: "EUR");

        var creditor = new NotificationRecipientDto(
            creditorId.ToString(),
            "admin@test.com",
            TelegramChatId: "987654321",
            WhatsAppPhone: "+34600000000",
            FirebaseDeviceToken: "fcm-token-xyz");

        _recipientRepository
            .GetByUserIdAsync(creditorId.ToString(), Arg.Any<CancellationToken>())
            .Returns(creditor);

        var context = Substitute.For<ConsumeContext<DebtSettledIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — se notifica por los 4 canales disponibles
        await _emailSender.Received(1).SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _telegramSender.Received(1).SendAsync(
            chatId: "987654321", Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _whatsAppSender.Received(1).SendAsync(
            phoneNumber: "+34600000000", Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _pushSender.Received(1).SendAsync(
            deviceToken: "fcm-token-xyz", Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
