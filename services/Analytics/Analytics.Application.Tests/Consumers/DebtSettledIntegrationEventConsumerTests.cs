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

public class DebtSettledIntegrationEventConsumerTests
{
    private readonly IDebtHistoryRepository _debtHistoryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DebtSettledIntegrationEventConsumer _consumer;

    public DebtSettledIntegrationEventConsumerTests()
    {
        _debtHistoryRepository = Substitute.For<IDebtHistoryRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _consumer = new DebtSettledIntegrationEventConsumer(
            _debtHistoryRepository,
            _unitOfWork);
    }

    [Fact]
    public async Task Consume_ConHistorialExistente_ActualizaDeudaSaldada()
    {
        // Arrange
        var userId = UserId.New();
        var existingHistory = DebtHistoryReadModel.Create(userId);
        existingHistory.AddDebt(10m);

        _debtHistoryRepository
            .GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(existingHistory);

        var evt = new DebtSettledIntegrationEvent
        {
            DebtId = Guid.NewGuid(),
            DebtorId = userId.Value,
            CreditorId = Guid.NewGuid(),
            Amount = 10m,
            Currency = "EUR"
        };

        var context = Substitute.For<ConsumeContext<DebtSettledIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — debe actualizar el historial y guardar cambios
        await _debtHistoryRepository.Received(1)
            .UpdateAsync(Arg.Any<DebtHistoryReadModel>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ConHistorialExistente_DecrementaPendingCount()
    {
        // Arrange
        var userId = UserId.New();
        var existingHistory = DebtHistoryReadModel.Create(userId);
        existingHistory.AddDebt(10m);
        existingHistory.AddDebt(5m); // 2 deudas pendientes

        _debtHistoryRepository
            .GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(existingHistory);

        var evt = new DebtSettledIntegrationEvent
        {
            DebtId = Guid.NewGuid(),
            DebtorId = userId.Value,
            CreditorId = Guid.NewGuid(),
            Amount = 10m,
            Currency = "EUR"
        };

        var context = Substitute.For<ConsumeContext<DebtSettledIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — el contador de pendientes debe haber bajado a 1
        existingHistory.PendingCount.Should().Be(1);
        existingHistory.TotalSettled.Should().Be(10m);
    }

    [Fact]
    public async Task Consume_SinHistorialExistente_RetornaSinProcesar()
    {
        // Arrange
        _debtHistoryRepository
            .GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns((DebtHistoryReadModel?)null);

        var evt = new DebtSettledIntegrationEvent
        {
            DebtId = Guid.NewGuid(),
            DebtorId = Guid.NewGuid(),
            CreditorId = Guid.NewGuid(),
            Amount = 10m,
            Currency = "EUR"
        };

        var context = Substitute.For<ConsumeContext<DebtSettledIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — no debe intentar guardar nada si no hay historial
        await _debtHistoryRepository.DidNotReceive()
            .UpdateAsync(Arg.Any<DebtHistoryReadModel>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Consume_ConUserIdInvalido_RetornaSinProcesar()
    {
        // Arrange
        var evt = new DebtSettledIntegrationEvent
        {
            DebtId = Guid.NewGuid(),
            DebtorId = Guid.Empty, // ID inválido
            CreditorId = Guid.NewGuid(),
            Amount = 10m,
            Currency = "EUR"
        };

        var context = Substitute.For<ConsumeContext<DebtSettledIntegrationEvent>>();
        context.Message.Returns(evt);
        context.CancellationToken.Returns(CancellationToken.None);

        // Act
        await _consumer.Consume(context);

        // Assert — no debe consultar ningún repositorio
        await _debtHistoryRepository.DidNotReceive()
            .GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
