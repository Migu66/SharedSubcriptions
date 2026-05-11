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

public class PaymentConfirmedIntegrationEventConsumerTests
{
    private readonly INotificationRecipientRepository _recipientRepository;
    private readonly IEmailSender _emailSender;
    private readonly ITelegramSender _telegramSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IPushNotificationSender _pushSender;
    private readonly PaymentConfirmedIntegrationEventConsumer _consumer;

    public PaymentConfirmedIntegrationEventConsumerTests()
    {
        _recipientRepository = Substitute.For<INotificationRecipientRepository>();
        _emailSender = Substitute.For<IEmailSender>();
        _telegramSender = Substitute.For<ITelegramSender>();
        _whatsAppSender = Substitute.For<IWhatsAppSender>();
        _pushSender = Substitute.For<IPushNotificationSender>();

        _emailSender.SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _consumer = new PaymentConfirmedIntegrationEventConsumer(
            _recipientRepository,
            _emailSender,
            _telegramSender,
            _whatsAppSender,
            _pushSender);
    }

    [Fact]
    public async Task Consume_ConCuotasYDestinatarios_EnviaEmailSoloADeudores()
    {
        // Arrange
        var groupId = GroupId.New();
        var member1Id = Guid.NewGuid();
        var member2Id = Guid.NewGuid();

        var quotas = new List<MemberQuotaDto>
        {
            new(member1Id, 3.33m, "EUR"),
            new(member2Id, 3.33m, "EUR")
        };

        var evt = new PaymentConfirmedIntegrationEvent(
            groupId: groupId,
            serviceName: "Netflix",
            totalAmount: 9.99m,
            currency: "EUR",
            quotas: quotas);

        var recipients = new List<NotificationRecipientDto>
        {
            new(member1Id.ToString(), "m1@test.com", null, null, null),
            new(member2Id.ToString(), "m2@test.com", null, null, null)
        };

        _recipientRepository
            .GetByGroupIdAsync(Arg.Any<GroupId>(), Arg.Any<CancellationToken>())
            .Returns(recipients);

        var context = Substitute.For<ConsumeContext<PaymentConfirmedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — se envía un email por cada deudor en la lista de cuotas
        await _emailSender.Received(2).SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_SinDestinatarios_NoEnviaNingunMensaje()
    {
        // Arrange
        var groupId = GroupId.New();
        var evt = new PaymentConfirmedIntegrationEvent(
            groupId: groupId,
            serviceName: "Spotify",
            totalAmount: 4.99m,
            currency: "EUR",
            quotas: new List<MemberQuotaDto> { new(Guid.NewGuid(), 4.99m, "EUR") });

        _recipientRepository
            .GetByGroupIdAsync(Arg.Any<GroupId>(), Arg.Any<CancellationToken>())
            .Returns(new List<NotificationRecipientDto>());

        var context = Substitute.For<ConsumeContext<PaymentConfirmedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — sin destinatarios no se llama a ningún sender
        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_MiembroNoEnListaDeCuotas_NoRecibeMensaje()
    {
        // Arrange
        var groupId = GroupId.New();
        var debtorId = Guid.NewGuid();
        var otherMemberId = Guid.NewGuid(); // está en el grupo pero no tiene cuota

        var quotas = new List<MemberQuotaDto>
        {
            new(debtorId, 5.00m, "EUR") // solo este tiene cuota
        };

        var evt = new PaymentConfirmedIntegrationEvent(
            groupId: groupId,
            serviceName: "Disney+",
            totalAmount: 5.00m,
            currency: "EUR",
            quotas: quotas);

        var recipients = new List<NotificationRecipientDto>
        {
            new(debtorId.ToString(), "debtor@test.com", null, null, null),
            new(otherMemberId.ToString(), "other@test.com", null, null, null)
        };

        _recipientRepository
            .GetByGroupIdAsync(Arg.Any<GroupId>(), Arg.Any<CancellationToken>())
            .Returns(recipients);

        var context = Substitute.For<ConsumeContext<PaymentConfirmedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — solo 1 email al deudor, no al otro miembro que no tiene cuota
        await _emailSender.Received(1).SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
