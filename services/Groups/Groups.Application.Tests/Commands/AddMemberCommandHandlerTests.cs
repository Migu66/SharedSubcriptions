using FluentAssertions;
using Groups.Application.Commands.AddMember;
using Groups.Domain.Aggregates;
using Groups.Domain.Errors;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Tests.Commands;

public class AddMemberCommandHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly AddMemberCommandHandler _handler;

    private static readonly UserId AdminId = UserId.New();
    private static readonly DateTime UtcNow = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);

    public AddMemberCommandHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();

        _dateTimeProvider.UtcNow.Returns(UtcNow);

        _handler = new AddMemberCommandHandler(
            _groupRepository,
            _unitOfWork,
            _dateTimeProvider);
    }

    private static Group CreateGroup()
    {
        var name = GroupName.Create("Grupo de prueba").Value;
        return Group.Create(name, AdminId, "admin@example.com", UtcNow).Value;
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaExito()
    {
        // Arrange
        var group = CreateGroup();
        var newMemberId = UserId.New();

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new AddMemberCommand(
            GroupId: group.Id,
            AdminId: AdminId,
            NewMemberId: newMemberId,
            InviteeEmail: "nuevo@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_LlamaSaveChangesAsync()
    {
        // Arrange
        var group = CreateGroup();
        var newMemberId = UserId.New();

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new AddMemberCommand(
            GroupId: group.Id,
            AdminId: AdminId,
            NewMemberId: newMemberId,
            InviteeEmail: "nuevo@example.com");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_GrupoNoExiste_RetornaErrorNotFound()
    {
        // Arrange
        var groupId = GroupId.New();

        _groupRepository.GetByIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns((Group?)null);

        var command = new AddMemberCommand(
            GroupId: groupId,
            AdminId: AdminId,
            NewMemberId: UserId.New(),
            InviteeEmail: "nuevo@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.NotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SolicitanteNoEsAdmin_RetornaErrorNotAdmin()
    {
        // Arrange
        var group = CreateGroup();
        var otroUserId = UserId.New();

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new AddMemberCommand(
            GroupId: group.Id,
            AdminId: otroUserId,      // no es el admin
            NewMemberId: UserId.New(),
            InviteeEmail: "nuevo@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.NotAdmin);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MiembroYaExiste_RetornaErrorMemberAlreadyExists()
    {
        // Arrange
        var group = CreateGroup();
        var newMemberId = UserId.New();

        // Añadimos el miembro una primera vez directamente sobre el agregado
        group.AddMember(newMemberId, "duplicado@example.com", UtcNow);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new AddMemberCommand(
            GroupId: group.Id,
            AdminId: AdminId,
            NewMemberId: newMemberId,  // mismo ID → duplicado
            InviteeEmail: "duplicado@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.MemberAlreadyExists);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
