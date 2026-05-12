using Analytics.Application.Consumers;
using Analytics.Application.IntegrationEvents;
using Analytics.Domain.ReadModels;
using Analytics.Domain.Repositories;
using Analytics.Domain.ValueObjects;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Tests.Consumers;

public class PaymentConfirmedIntegrationEventConsumerTests
{
    private readonly IGroupSavingsRepository _groupSavingsRepository;
    private readonly IServiceSpendingRepository _serviceSpendingRepository;
    private readonly ISubscriptionContextRepository _subscriptionContextRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PaymentConfirmedIntegrationEventConsumer _consumer;

    public PaymentConfirmedIntegrationEventConsumerTests()
    {
        _groupSavingsRepository = Substitute.For<IGroupSavingsRepository>();
        _serviceSpendingRepository = Substitute.For<IServiceSpendingRepository>();
        _subscriptionContextRepository = Substitute.For<ISubscriptionContextRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _consumer = new PaymentConfirmedIntegrationEventConsumer(
            _groupSavingsRepository,
            _serviceSpendingRepository,
            _subscriptionContextRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Consume_SinReadModelExistente_CreaNuevoGroupSavings()
    {
        // Arrange
        var groupId = GroupId.New();
        var subscriptionId = Guid.NewGuid();

        _groupSavingsRepository
            .GetByGroupIdAndYearAsync(Arg.Any<GroupId>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((GroupSavingsReadModel?)null);

        _serviceSpendingRepository
            .GetByGroupIdAndServiceNameAsync(Arg.Any<GroupId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ServiceSpendingReadModel?)null);

        _subscriptionContextRepository
            .GetBySubscriptionIdAsync(subscriptionId, Arg.Any<CancellationToken>())
            .Returns((SubscriptionContextReadModel?)null);

        var evt = new PaymentConfirmedIntegrationEvent
        {
            PaymentRecordId = Guid.NewGuid(),
            SubscriptionId = subscriptionId,
            GroupId = groupId.Value,
            AdminId = Guid.NewGuid(),
            TotalAmount = 9.99m,
            Currency = "EUR",
            Quotas = [new MemberQuotaAnalyticsDto(Guid.NewGuid(), 3.33m, "EUR", false)]
        };

        var context = Substitute.For<ConsumeContext<PaymentConfirmedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — al ser nuevo debe llamar a AddAsync para GroupSavings
        await _groupSavingsRepository.Received(1)
            .AddAsync(Arg.Any<GroupSavingsReadModel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ConReadModelExistente_ActualizaGroupSavings()
    {
        // Arrange
        var groupId = GroupId.New();
        var existingModel = GroupSavingsReadModel.Create(groupId, DateTime.UtcNow.Year);
        existingModel.AddPayment(5m, 1m); // ya tiene datos previos

        _groupSavingsRepository
            .GetByGroupIdAndYearAsync(Arg.Any<GroupId>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(existingModel);

        _serviceSpendingRepository
            .GetByGroupIdAndServiceNameAsync(Arg.Any<GroupId>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((ServiceSpendingReadModel?)null);

        _subscriptionContextRepository
            .GetBySubscriptionIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((SubscriptionContextReadModel?)null);

        var evt = new PaymentConfirmedIntegrationEvent
        {
            PaymentRecordId = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            GroupId = groupId.Value,
            AdminId = Guid.NewGuid(),
            TotalAmount = 9.99m,
            Currency = "EUR",
            Quotas = []
        };

        var context = Substitute.For<ConsumeContext<PaymentConfirmedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — como ya existe debe llamar a UpdateAsync, no a AddAsync
        await _groupSavingsRepository.Received(1)
            .UpdateAsync(Arg.Any<GroupSavingsReadModel>(), Arg.Any<CancellationToken>());
        await _groupSavingsRepository.DidNotReceive()
            .AddAsync(Arg.Any<GroupSavingsReadModel>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ConContextoDeSuscripcion_UsaNombreServicioReal()
    {
        // Arrange
        var groupId = GroupId.New();
        var subscriptionId = Guid.NewGuid();

        var subscriptionContext = SubscriptionContextReadModel.Create(
            subscriptionId, groupId, "Netflix");

        _groupSavingsRepository
            .GetByGroupIdAndYearAsync(Arg.Any<GroupId>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((GroupSavingsReadModel?)null);

        _serviceSpendingRepository
            .GetByGroupIdAndServiceNameAsync(groupId, "Netflix", Arg.Any<CancellationToken>())
            .Returns((ServiceSpendingReadModel?)null);

        _subscriptionContextRepository
            .GetBySubscriptionIdAsync(subscriptionId, Arg.Any<CancellationToken>())
            .Returns(subscriptionContext);

        var evt = new PaymentConfirmedIntegrationEvent
        {
            PaymentRecordId = Guid.NewGuid(),
            SubscriptionId = subscriptionId,
            GroupId = groupId.Value,
            AdminId = Guid.NewGuid(),
            TotalAmount = 9.99m,
            Currency = "EUR",
            Quotas = []
        };

        var context = Substitute.For<ConsumeContext<PaymentConfirmedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — debe consultar ServiceSpending por el nombre correcto "Netflix"
        await _serviceSpendingRepository.Received(1)
            .GetByGroupIdAndServiceNameAsync(
                Arg.Any<GroupId>(), "Netflix", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ConGrupIdInvalido_RetornaSinProcesar()
    {
        // Arrange
        var evt = new PaymentConfirmedIntegrationEvent
        {
            PaymentRecordId = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            GroupId = Guid.Empty, // ID inválido
            AdminId = Guid.NewGuid(),
            TotalAmount = 9.99m,
            Currency = "EUR",
            Quotas = []
        };

        var context = Substitute.For<ConsumeContext<PaymentConfirmedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — no debe tocar ningún repositorio
        await _groupSavingsRepository.DidNotReceive()
            .GetByGroupIdAndYearAsync(Arg.Any<GroupId>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
