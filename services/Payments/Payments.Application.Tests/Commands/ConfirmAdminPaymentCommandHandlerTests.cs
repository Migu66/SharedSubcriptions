using FluentAssertions;
using NSubstitute;
using Payments.Application.Commands.ConfirmAdminPayment;
using Payments.Domain.Aggregates;
using Payments.Domain.Repositories;
using Payments.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Application.Tests.Commands;

public class ConfirmAdminPaymentCommandHandlerTests
{
    private readonly IPaymentRecordRepository _paymentRecordRepository;
    private readonly IDebtRepository _debtRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ConfirmAdminPaymentCommandHandler _handler;

    public ConfirmAdminPaymentCommandHandlerTests()
    {
        _paymentRecordRepository = Substitute.For<IPaymentRecordRepository>();
        _debtRepository = Substitute.For<IDebtRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _handler = new ConfirmAdminPaymentCommandHandler(
            _paymentRecordRepository,
            _debtRepository,
            _unitOfWork,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaPaymentRecordId()
    {
        // Arrange
        var adminId = UserId.New();
        var member1 = UserId.New();
        var member2 = UserId.New();

        var command = new ConfirmAdminPaymentCommand(
            SubscriptionId: SubscriptionId.New(),
            GroupId: GroupId.New(),
            AdminId: adminId,
            MemberIds: [adminId.Value, member1.Value, member2.Value],
            TotalAmount: 9.99m,
            Currency: "EUR",
            PaidAt: DateTime.UtcNow);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(default);
    }

    [Fact]
    public async Task Handle_ConTresMiembros_CreaDosDeudasExcluyendoAdmin()
    {
        // Arrange
        var adminId = UserId.New();
        var member1 = UserId.New();
        var member2 = UserId.New();

        var command = new ConfirmAdminPaymentCommand(
            SubscriptionId: SubscriptionId.New(),
            GroupId: GroupId.New(),
            AdminId: adminId,
            MemberIds: [adminId.Value, member1.Value, member2.Value],
            TotalAmount: 9.99m,
            Currency: "EUR",
            PaidAt: DateTime.UtcNow);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — debe crear 2 deudas (los 2 miembros que no son el admin)
        await _debtRepository.Received(2).AddAsync(Arg.Any<Debt>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConDatosValidos_LlamaSaveChangesAsync()
    {
        // Arrange
        var adminId = UserId.New();
        var command = new ConfirmAdminPaymentCommand(
            SubscriptionId: SubscriptionId.New(),
            GroupId: GroupId.New(),
            AdminId: adminId,
            MemberIds: [adminId.Value, UserId.New().Value],
            TotalAmount: 9.99m,
            Currency: "EUR",
            PaidAt: DateTime.UtcNow);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConImporteNegativo_RetornaFallo()
    {
        // Arrange
        var adminId = UserId.New();
        var command = new ConfirmAdminPaymentCommand(
            SubscriptionId: SubscriptionId.New(),
            GroupId: GroupId.New(),
            AdminId: adminId,
            MemberIds: [adminId.Value],
            TotalAmount: -1m,
            Currency: "EUR",
            PaidAt: DateTime.UtcNow);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
