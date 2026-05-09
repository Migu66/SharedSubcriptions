using FluentAssertions;
using Identity.Domain.Aggregates;
using Identity.Domain.Errors;
using Identity.Domain.Events;

namespace Identity.Domain.Tests.Aggregates;

public class ApplicationUserTests
{
    private static readonly DateTime UtcNow = new(2026, 5, 9, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_ConDatosValidos_RetornaUsuarioExitoso()
    {
        // Arrange
        const string email = "juan@example.com";
        const string firstName = "Juan";
        const string lastName = "García";

        // Act
        var result = ApplicationUser.Create(email, firstName, lastName, UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be(email);
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.CreatedAt.Should().Be(UtcNow);
    }

    [Fact]
    public void Create_ConDatosValidos_EmiteUserRegisteredEvent()
    {
        // Arrange
        const string email = "juan@example.com";

        // Act
        var result = ApplicationUser.Create(email, "Juan", "García", UtcNow);

        // Assert
        result.Value.DomainEvents.Should().ContainSingle();
        result.Value.DomainEvents.First().Should().BeOfType<UserRegisteredEvent>();

        var evt = (UserRegisteredEvent)result.Value.DomainEvents.First();
        evt.Email.Should().Be(email);
        evt.FirstName.Should().Be("Juan");
        evt.LastName.Should().Be("García");
    }

    [Fact]
    public void Create_ConEmailVacio_RetornaError()
    {
        // Arrange & Act
        var result = ApplicationUser.Create(string.Empty, "Juan", "García", UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_ConEmailSinArroba_RetornaError()
    {
        // Arrange & Act
        var result = ApplicationUser.Create("emailsinelformato", "Juan", "García", UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.InvalidEmail);
    }

    [Fact]
    public void Create_ConNombreVacio_RetornaError()
    {
        // Arrange & Act
        var result = ApplicationUser.Create("juan@example.com", string.Empty, "García", UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Create_ConApellidoVacio_RetornaError()
    {
        // Arrange & Act
        var result = ApplicationUser.Create("juan@example.com", "Juan", string.Empty, UtcNow);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void ClearDomainEvents_DespuesDeCrear_VaciaLaLista()
    {
        // Arrange
        var user = ApplicationUser.Create("juan@example.com", "Juan", "García", UtcNow).Value;

        // Act
        user.ClearDomainEvents();

        // Assert
        user.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Create_ConDatosValidos_IdNoEsVacio()
    {
        // Arrange & Act
        var result = ApplicationUser.Create("juan@example.com", "Juan", "García", UtcNow);

        // Assert
        result.Value.Id.Should().NotBe(Guid.Empty);
    }
}
