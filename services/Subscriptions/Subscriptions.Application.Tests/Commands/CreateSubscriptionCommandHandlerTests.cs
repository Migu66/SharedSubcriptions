using FluentAssertions;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Application.Commands.CreateSubscription;
using Subscriptions.Domain.Aggregates;
using Subscriptions.Domain.Enums;
using Subscriptions.Domain.Errors;
using Subscriptions.Domain.Repositories;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Tests.Commands;

public class CreateSubscriptionCommandHandlerTests
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CreateSubscriptionCommandHandler _handler;

    private static readonly DateTime UtcNow = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);

    public CreateSubscriptionCommandHandlerTests()
    {
        _subscriptionRepository = Substitute.For<ISubscriptionRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(UtcNow);

        _handler = new CreateSubscriptionCommandHandler(
            _subscriptionRepository,
            _unitOfWork,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaSubscriptionIdExitoso()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(
            GroupId: GroupId.New(),
            AdminId: UserId.New(),
            ServiceName: "Netflix",
            TotalCost: 15.99m,
            Currency: "EUR",
            BillingCycle: BillingCycle.Monthly,
            FirstBillingDate: UtcNow.AddMonths(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_LlamaAddAsyncEnElRepositorio()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(
            GroupId: GroupId.New(),
            AdminId: UserId.New(),
            ServiceName: "Spotify",
            TotalCost: 9.99m,
            Currency: "EUR",
            BillingCycle: BillingCycle.Monthly,
            FirstBillingDate: UtcNow.AddMonths(1));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _subscriptionRepository.Received(1).AddAsync(
            Arg.Any<Subscription>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConDatosValidos_LlamaSaveChangesAsync()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(
            GroupId: GroupId.New(),
            AdminId: UserId.New(),
            ServiceName: "Disney+",
            TotalCost: 8.99m,
            Currency: "USD",
            BillingCycle: BillingCycle.Annual,
            FirstBillingDate: UtcNow.AddYears(1));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConNombreVacio_RetornaErrorSinLlamarRepositorio()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(
            GroupId: GroupId.New(),
            AdminId: UserId.New(),
            ServiceName: "",
            TotalCost: 9.99m,
            Currency: "EUR",
            BillingCycle: BillingCycle.Monthly,
            FirstBillingDate: UtcNow.AddMonths(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(SubscriptionErrors.ServiceNameEmpty.Code);
        await _subscriptionRepository.DidNotReceive().AddAsync(
            Arg.Any<Subscription>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConImporteNegativo_RetornaErrorSinLlamarRepositorio()
    {
        // Arrange
        var command = new CreateSubscriptionCommand(
            GroupId: GroupId.New(),
            AdminId: UserId.New(),
            ServiceName: "Netflix",
            TotalCost: -5m,
            Currency: "EUR",
            BillingCycle: BillingCycle.Monthly,
            FirstBillingDate: UtcNow.AddMonths(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(MoneyErrors.NegativeAmount.Code);
    }
}
