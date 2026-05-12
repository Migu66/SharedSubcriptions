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

public class DebtCreatedIntegrationEventConsumerTests
{
    private readonly IDebtHistoryRepository _debtHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DebtCreatedIntegrationEventConsumer _consumer;

    public DebtCreatedIntegrationEventConsumerTests()
    {
        _debtHistoryRepository = Substitute.For<IDebtHistoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _consumer = new DebtCreatedIntegrationEventConsumer(
            _debtHistoryRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Consume_SinHistorialPrevio_CreaHistorialNuevo()
    {
        // Arrange
        _debtHistoryRepository
            .GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns((DebtHistoryReadModel?)null);

        var evt = new DebtCreatedIntegrationEvent
        {
            DebtId = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            DebtorId = Guid.NewGuid(),
            CreditorId = Guid.NewGuid(),
            Amount = 5m,
            Currency = "EUR"
        };

        var context = Substitute.For<ConsumeContext<DebtCreatedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — al ser nuevo debe llamar a AddAsync
        await _debtHistoryRepository.Received(1)
            .AddAsync(Arg.Any<DebtHistoryReadModel>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ConHistorialExistente_ActualizaContadorPendientes()
    {
        // Arrange
        var userId = UserId.New();
        var existingHistory = DebtHistoryReadModel.Create(userId);
        existingHistory.AddDebt(10m); // ya tiene 1 deuda

        _debtHistoryRepository
            .GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(existingHistory);

        var evt = new DebtCreatedIntegrationEvent
        {
            DebtId = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            DebtorId = userId.Value,
            CreditorId = Guid.NewGuid(),
            Amount = 5m,
            Currency = "EUR"
        };

        var context = Substitute.For<ConsumeContext<DebtCreatedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — debe actualizar (no añadir) y el contador debe ser 2
        await _debtHistoryRepository.Received(1)
            .UpdateAsync(Arg.Any<DebtHistoryReadModel>(), Arg.Any<CancellationToken>());
        existingHistory.PendingCount.Should().Be(2);
        existingHistory.TotalDebt.Should().Be(15m);
    }

    [Fact]
    public async Task Consume_ConUserIdInvalido_RetornaSinProcesar()
    {
        // Arrange
        var evt = new DebtCreatedIntegrationEvent
        {
            DebtId = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            DebtorId = Guid.Empty, // ID inválido
            CreditorId = Guid.NewGuid(),
            Amount = 5m,
            Currency = "EUR"
        };

        var context = Substitute.For<ConsumeContext<DebtCreatedIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — no debe tocar ningún repositorio
        await _debtHistoryRepository.DidNotReceive()
            .GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
