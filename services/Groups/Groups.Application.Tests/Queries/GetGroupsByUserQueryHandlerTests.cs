using FluentAssertions;
using Groups.Application.Queries.GetGroupsByUser;
using Groups.Domain.Aggregates;
using Groups.Domain.Enums;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Tests.Queries;

public class GetGroupsByUserQueryHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly GetGroupsByUserQueryHandler _handler;

    private static readonly DateTime UtcNow = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);

    public GetGroupsByUserQueryHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _handler = new GetGroupsByUserQueryHandler(_groupRepository);
    }

    private static Group CreateGroupAsAdmin(UserId adminId)
    {
        var name = GroupName.Create("Grupo Admin").Value;
        return Group.Create(name, adminId, "admin@example.com", UtcNow).Value;
    }

    private static Group CreateGroupWithMember(UserId adminId, UserId memberId)
    {
        var name = GroupName.Create("Grupo Miembro").Value;
        var group = Group.Create(name, adminId, "admin2@example.com", UtcNow).Value;
        group.AddMember(memberId, "miembro@example.com", UtcNow);
        return group;
    }

    [Fact]
    public async Task Handle_UsuarioSinGrupos_RetornaListaVacia()
    {
        // Arrange
        var userId = UserId.New();

        _groupRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Group>().AsReadOnly());

        var query = new GetGroupsByUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UsuarioEsAdmin_RetornaRolAdmin()
    {
        // Arrange
        var adminId = UserId.New();
        var group = CreateGroupAsAdmin(adminId);

        _groupRepository.GetByUserIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(new List<Group> { group }.AsReadOnly());

        var query = new GetGroupsByUserQuery(adminId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().UserRole.Should().Be(nameof(GroupRole.Admin));
    }

    [Fact]
    public async Task Handle_UsuarioEsMiembro_RetornaRolMember()
    {
        // Arrange
        var adminId = UserId.New();
        var memberId = UserId.New();
        var group = CreateGroupWithMember(adminId, memberId);

        _groupRepository.GetByUserIdAsync(memberId, Arg.Any<CancellationToken>())
            .Returns(new List<Group> { group }.AsReadOnly());

        var query = new GetGroupsByUserQuery(memberId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.First().UserRole.Should().Be(nameof(GroupRole.Member));
    }

    [Fact]
    public async Task Handle_VariosGrupos_RetornaTodosConDatosCorrectos()
    {
        // Arrange
        var userId = UserId.New();
        var grupo1 = CreateGroupAsAdmin(userId);
        var grupo2 = CreateGroupWithMember(UserId.New(), userId);

        _groupRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(new List<Group> { grupo1, grupo2 }.AsReadOnly());

        var query = new GetGroupsByUserQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(g => g.UserRole == nameof(GroupRole.Admin));
        result.Value.Should().Contain(g => g.UserRole == nameof(GroupRole.Member));
    }

    [Fact]
    public async Task Handle_GrupoConVariosMiembros_RetornaMemberCountCorrecto()
    {
        // Arrange
        var adminId = UserId.New();
        var group = CreateGroupAsAdmin(adminId);
        group.AddMember(UserId.New(), "m1@example.com", UtcNow);
        group.AddMember(UserId.New(), "m2@example.com", UtcNow);

        _groupRepository.GetByUserIdAsync(adminId, Arg.Any<CancellationToken>())
            .Returns(new List<Group> { group }.AsReadOnly());

        var query = new GetGroupsByUserQuery(adminId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.First().MemberCount.Should().Be(3); // admin + 2 miembros
    }
}
