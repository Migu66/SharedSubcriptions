using FluentAssertions;
using Groups.Application.Commands.RemoveMember;
using Groups.Domain.Aggregates;
using Groups.Domain.Errors;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Tests.Commands;

public class RemoveMemberCommandHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly RemoveMemberCommandHandler _handler;

    private static readonly UserId AdminId = UserId.New();
    private static readonly DateTime UtcNow = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);

    public RemoveMemberCommandHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new RemoveMemberCommandHandler(
            _groupRepository,
            _unitOfWork);
    }

    private static Group CreateGroupWithMember(out UserId memberId)
    {
        var name = GroupName.Create("Grupo de prueba").Value;
        var group = Group.Create(name, AdminId, "admin@example.com", UtcNow).Value;
        memberId = UserId.New();
        group.AddMember(memberId, "miembro@example.com", UtcNow);
        group.ClearDomainEvents();
        return group;
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaExito()
    {
        // Arrange
        var group = CreateGroupWithMember(out var memberId);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new RemoveMemberCommand(
            GroupId: group.Id,
            AdminId: AdminId,
            MemberToRemoveId: memberId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_LlamaSaveChangesAsync()
    {
        // Arrange
        var group = CreateGroupWithMember(out var memberId);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new RemoveMemberCommand(
            GroupId: group.Id,
            AdminId: AdminId,
            MemberToRemoveId: memberId);

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

        var command = new RemoveMemberCommand(
            GroupId: groupId,
            AdminId: AdminId,
            MemberToRemoveId: UserId.New());

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
        var group = CreateGroupWithMember(out var memberId);
        var otroUserId = UserId.New();

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new RemoveMemberCommand(
            GroupId: group.Id,
            AdminId: otroUserId,      // no es el admin
            MemberToRemoveId: memberId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.NotAdmin);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_IntentandoEliminarAdmin_RetornaErrorAdminCannotBeRemoved()
    {
        // Arrange
        var group = CreateGroupWithMember(out _);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new RemoveMemberCommand(
            GroupId: group.Id,
            AdminId: AdminId,
            MemberToRemoveId: AdminId);  // intentar eliminar al propio admin

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.AdminCannotBeRemoved);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MiembroInexistente_RetornaErrorMemberNotFound()
    {
        // Arrange
        var group = CreateGroupWithMember(out _);
        var inexistenteId = UserId.New();

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var command = new RemoveMemberCommand(
            GroupId: group.Id,
            AdminId: AdminId,
            MemberToRemoveId: inexistenteId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.MemberNotFound);
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
