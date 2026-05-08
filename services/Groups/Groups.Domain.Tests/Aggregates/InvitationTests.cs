using FluentAssertions;
using Groups.Domain.Aggregates;
using Groups.Domain.Enums;
using Groups.Domain.Errors;
using Groups.Domain.ValueObjects;

namespace Groups.Domain.Tests.Aggregates;

public class InvitationTests
{
    private static readonly GroupId GroupId = GroupId.New();
    private static readonly DateTime CreatedAt = new(2026, 5, 8, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime ExpiresAt = CreatedAt.AddDays(7);

    private static Invitation CreateValidInvitation() =>
        Invitation.Create(GroupId, "invitado@example.com", CreatedAt, ExpiresAt).Value;

    [Fact]
    public void Create_ConDatosValidos_RetornaInvitacionExitosa()
    {
        // Arrange & Act
        var result = Invitation.Create(GroupId, "invitado@example.com", CreatedAt, ExpiresAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.InviteeEmail.Should().Be("invitado@example.com");
        result.Value.GroupId.Should().Be(GroupId);
        result.Value.Status.Should().Be(InvitationStatus.Pending);
        result.Value.CreatedAt.Should().Be(CreatedAt);
        result.Value.ExpiresAt.Should().Be(ExpiresAt);
    }

    [Fact]
    public void Create_ConEmailVacio_RetornaError()
    {
        // Arrange & Act
        var result = Invitation.Create(GroupId, "", CreatedAt, ExpiresAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.EmailEmpty);
    }

    [Fact]
    public void Create_ConFechaExpiracionAnteriorACreacion_RetornaError()
    {
        // Arrange
        var fechaExpiracionInvalida = CreatedAt.AddSeconds(-1);

        // Act
        var result = Invitation.Create(GroupId, "invitado@example.com", CreatedAt, fechaExpiracionInvalida);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.InvalidExpiryDate);
    }

    [Fact]
    public void Accept_InvitacionPendiente_RetornaExitoYCambiaEstado()
    {
        // Arrange
        var invitation = CreateValidInvitation();
        var now = CreatedAt.AddDays(1);

        // Act
        var result = invitation.Accept(now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        invitation.Status.Should().Be(InvitationStatus.Accepted);
    }

    [Fact]
    public void Accept_InvitacionYaAceptada_RetornaError()
    {
        // Arrange
        var invitation = CreateValidInvitation();
        invitation.Accept(CreatedAt.AddDays(1));

        // Act
        var result = invitation.Accept(CreatedAt.AddDays(2));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.AlreadyAccepted);
    }

    [Fact]
    public void Accept_InvitacionCancelada_RetornaError()
    {
        // Arrange
        var invitation = CreateValidInvitation();
        invitation.Cancel();

        // Act
        var result = invitation.Accept(CreatedAt.AddDays(1));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.AlreadyCancelled);
    }

    [Fact]
    public void Accept_InvitacionExpirada_RetornaError()
    {
        // Arrange
        var invitation = CreateValidInvitation();
        var despuesDeExpirar = ExpiresAt.AddSeconds(1);

        // Act
        var result = invitation.Accept(despuesDeExpirar);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.Expired);
    }

    [Fact]
    public void Cancel_InvitacionPendiente_RetornaExitoYCambiaEstado()
    {
        // Arrange
        var invitation = CreateValidInvitation();

        // Act
        var result = invitation.Cancel();

        // Assert
        result.IsSuccess.Should().BeTrue();
        invitation.Status.Should().Be(InvitationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_InvitacionYaAceptada_RetornaError()
    {
        // Arrange
        var invitation = CreateValidInvitation();
        invitation.Accept(CreatedAt.AddDays(1));

        // Act
        var result = invitation.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.AlreadyAccepted);
    }

    [Fact]
    public void Cancel_InvitacionYaCancelada_RetornaError()
    {
        // Arrange
        var invitation = CreateValidInvitation();
        invitation.Cancel();

        // Act
        var result = invitation.Cancel();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvitationErrors.AlreadyCancelled);
    }
}
