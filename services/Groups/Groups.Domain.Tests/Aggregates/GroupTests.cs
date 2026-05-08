using FluentAssertions;
using Groups.Domain.Aggregates;
using Groups.Domain.Errors;
using Groups.Domain.Events;
using Groups.Domain.ValueObjects;

namespace Groups.Domain.Tests.Aggregates;

public class GroupTests
{
    private static readonly UserId AdminId = UserId.New();
    private const string AdminEmail = "admin@example.com";
    private static readonly DateTime Now = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);

    private static Group CreateValidGroup()
    {
        var name = GroupName.Create("Mi Grupo de Netflix").Value;
        return Group.Create(name, AdminId, AdminEmail, Now).Value;
    }

    [Fact]
    public void Create_ConDatosValidos_RetornaGrupoExitoso()
    {
        // Arrange
        var name = GroupName.Create("Mi Grupo de Netflix").Value;

        // Act
        var result = Group.Create(name, AdminId, AdminEmail, Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.AdminId.Should().Be(AdminId);
        result.Value.CreatedAt.Should().Be(Now);
    }

    [Fact]
    public void Create_ConDatosValidos_AñadeAdminComoMiembro()
    {
        // Arrange
        var name = GroupName.Create("Mi Grupo").Value;

        // Act
        var result = Group.Create(name, AdminId, AdminEmail, Now);

        // Assert
        result.Value.Members.Should().HaveCount(1);
        result.Value.Members.First().Email.Should().Be(AdminEmail);
    }

    [Fact]
    public void Create_ConDatosValidos_EmiteGroupCreatedEvent()
    {
        // Arrange
        var name = GroupName.Create("Mi Grupo").Value;

        // Act
        var result = Group.Create(name, AdminId, AdminEmail, Now);

        // Assert
        result.Value.DomainEvents.Should().ContainSingle();
        result.Value.DomainEvents.First().Should().BeOfType<GroupCreatedEvent>();

        var evt = (GroupCreatedEvent)result.Value.DomainEvents.First();
        evt.AdminId.Should().Be(AdminId);
    }

    [Fact]
    public void AddMember_ConMiembroNuevo_RetornaExito()
    {
        // Arrange
        var group = CreateValidGroup();
        var newMemberId = UserId.New();

        // Act
        var result = group.AddMember(newMemberId, "nuevo@example.com", Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void AddMember_ConMiembroNuevo_AñadeAlColeccion()
    {
        // Arrange
        var group = CreateValidGroup();
        var newMemberId = UserId.New();

        // Act
        group.AddMember(newMemberId, "nuevo@example.com", Now);

        // Assert
        group.Members.Should().HaveCount(2);
        group.Members.Should().Contain(m => m.Email == "nuevo@example.com");
    }

    [Fact]
    public void AddMember_ConMiembroNuevo_EmiteMemberAddedEvent()
    {
        // Arrange
        var group = CreateValidGroup();
        group.ClearDomainEvents();
        var newMemberId = UserId.New();

        // Act
        group.AddMember(newMemberId, "nuevo@example.com", Now);

        // Assert
        group.DomainEvents.Should().ContainSingle();
        group.DomainEvents.First().Should().BeOfType<MemberAddedEvent>();

        var evt = (MemberAddedEvent)group.DomainEvents.First();
        evt.UserId.Should().Be(newMemberId);
        evt.Email.Should().Be("nuevo@example.com");
    }

    [Fact]
    public void AddMember_ConMiembroDuplicado_RetornaError()
    {
        // Arrange
        var group = CreateValidGroup();
        var newMemberId = UserId.New();
        group.AddMember(newMemberId, "duplicado@example.com", Now);
        group.ClearDomainEvents();

        // Act
        var result = group.AddMember(newMemberId, "duplicado@example.com", Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.MemberAlreadyExists);
    }

    [Fact]
    public void RemoveMember_ConMiembroExistente_RetornaExito()
    {
        // Arrange
        var group = CreateValidGroup();
        var memberId = UserId.New();
        group.AddMember(memberId, "miembro@example.com", Now);
        group.ClearDomainEvents();

        // Act
        var result = group.RemoveMember(memberId);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void RemoveMember_ConMiembroExistente_EliminaDelColeccion()
    {
        // Arrange
        var group = CreateValidGroup();
        var memberId = UserId.New();
        group.AddMember(memberId, "miembro@example.com", Now);

        // Act
        group.RemoveMember(memberId);

        // Assert
        group.Members.Should().HaveCount(1);
        group.Members.Should().NotContain(m => m.Email == "miembro@example.com");
    }

    [Fact]
    public void RemoveMember_ConMiembroExistente_EmiteMemberRemovedEvent()
    {
        // Arrange
        var group = CreateValidGroup();
        var memberId = UserId.New();
        group.AddMember(memberId, "miembro@example.com", Now);
        group.ClearDomainEvents();

        // Act
        group.RemoveMember(memberId);

        // Assert
        group.DomainEvents.Should().ContainSingle();
        group.DomainEvents.First().Should().BeOfType<MemberRemovedEvent>();

        var evt = (MemberRemovedEvent)group.DomainEvents.First();
        evt.UserId.Should().Be(memberId);
    }

    [Fact]
    public void RemoveMember_IntentandoEliminarAdmin_RetornaError()
    {
        // Arrange
        var group = CreateValidGroup();
        group.ClearDomainEvents();

        // Act
        var result = group.RemoveMember(AdminId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.AdminCannotBeRemoved);
    }

    [Fact]
    public void RemoveMember_MiembroInexistente_RetornaError()
    {
        // Arrange
        var group = CreateValidGroup();
        var inexistenteMemberId = UserId.New();

        // Act
        var result = group.RemoveMember(inexistenteMemberId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupErrors.MemberNotFound);
    }
}
