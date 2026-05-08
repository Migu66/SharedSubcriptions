using FluentAssertions;
using Groups.Application.Queries.GetGroupDetails;
using Groups.Domain.Aggregates;
using Groups.Domain.Errors;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Tests.Queries;

public class GetGroupDetailsQueryHandlerTests
{
    private readonly IGroupRepository _groupRepository;
    private readonly GetGroupDetailsQueryHandler _handler;

    private static readonly UserId AdminId = UserId.New();
    private static readonly DateTime UtcNow = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);

    public GetGroupDetailsQueryHandlerTests()
    {
        _groupRepository = Substitute.For<IGroupRepository>();
        _handler = new GetGroupDetailsQueryHandler(_groupRepository);
    }

    private static Group CreateGroup()
    {
        var name = GroupName.Create("Grupo de prueba").Value;
        return Group.Create(name, AdminId, "admin@example.com", UtcNow).Value;
    }

    [Fact]
    public async Task Handle_GrupoExiste_RetornaDtoConDatosCorrectos()
    {
        // Arrange
        var group = CreateGroup();
        var memberId = UserId.New();
        group.AddMember(memberId, "miembro@example.com", UtcNow);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var query = new GetGroupDetailsQuery(group.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(group.Id);
        result.Value.Name.Should().Be("Grupo de prueba");
        result.Value.AdminId.Should().Be(AdminId);
        result.Value.CreatedAt.Should().Be(UtcNow);
    }

    [Fact]
    public async Task Handle_GrupoExiste_RetornaMiembrosProyectados()
    {
        // Arrange
        var group = CreateGroup();
        var memberId = UserId.New();
        group.AddMember(memberId, "miembro@example.com", UtcNow);

        _groupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);

        var query = new GetGroupDetailsQuery(group.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Value.Members.Should().HaveCount(2);  // admin + 1 miembro
        result.Value.Members.Should().Contain(m => m.Email == "miembro@example.com");
        result.Value.Members.Should().Contain(m => m.Role == "Admin");
        result.Value.Members.Should().Contain(m => m.Role == "Member");
    }

    [Fact]
    public async Task Handle_GrupoNoExiste_RetornaErrorNotFound()
    {
        // Arrange
        var groupId = GroupId.New();

        _groupRepository.GetByIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns((Group?)null);

        var query = new GetGroupDetailsQuery(groupId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.NotFound);
    }
}
