using FluentAssertions;
using Groups.Application.Commands.CreateGroup;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Tests.Commands;

public class CreateGroupCommandHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly CreateGroupCommandHandler _handler;

    private static readonly DateTime UtcNow = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);

    public CreateGroupCommandHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _dateTimeProvider.UtcNow.Returns(UtcNow);

        _handler = new CreateGroupCommandHandler(
            _groupRepository,
            _unitOfWork,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaGroupIdExitoso()
    {
        // Arrange
        var command = new CreateGroupCommand(
            Name: "Familia Netflix",
            AdminId: UserId.New(),
            AdminEmail: "admin@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_LlamaAddAsyncEnElRepositorio()
    {
        // Arrange
        var command = new CreateGroupCommand(
            Name: "Familia Netflix",
            AdminId: UserId.New(),
            AdminEmail: "admin@example.com");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _groupRepository.Received(1).AddAsync(
            Arg.Any<Domain.Aggregates.Group>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConDatosValidos_LlamaSaveChangesAsync()
    {
        // Arrange
        var command = new CreateGroupCommand(
            Name: "Familia Netflix",
            AdminId: UserId.New(),
            AdminEmail: "admin@example.com");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConNombreVacio_RetornaErrorSinLlamarRepositorio()
    {
        // Arrange
        var command = new CreateGroupCommand(
            Name: "",
            AdminId: UserId.New(),
            AdminEmail: "admin@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _groupRepository.DidNotReceive().AddAsync(
            Arg.Any<Domain.Aggregates.Group>(),
            Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConNombreMenorDe3Caracteres_RetornaErrorSinLlamarRepositorio()
    {
        // Arrange
        var command = new CreateGroupCommand(
            Name: "AB",
            AdminId: UserId.New(),
            AdminEmail: "admin@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _groupRepository.DidNotReceive().AddAsync(
            Arg.Any<Domain.Aggregates.Group>(),
            Arg.Any<CancellationToken>());
    }
}
