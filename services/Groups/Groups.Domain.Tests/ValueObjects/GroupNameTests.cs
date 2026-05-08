using FluentAssertions;
using Groups.Domain.Errors;
using Groups.Domain.ValueObjects;

namespace Groups.Domain.Tests.ValueObjects;

public class GroupNameTests
{
    [Fact]
    public void Create_ConNombreValido_RetornaExito()
    {
        // Arrange
        const string nombre = "Familia Netflix";

        // Act
        var result = GroupName.Create(nombre);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(nombre);
    }

    [Fact]
    public void Create_ConNombreVacio_RetornaError()
    {
        // Arrange
        const string nombre = "";

        // Act
        var result = GroupName.Create(nombre);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupNameErrors.Empty);
    }

    [Fact]
    public void Create_ConNombreNull_RetornaError()
    {
        // Arrange
        string? nombre = null;

        // Act
        var result = GroupName.Create(nombre);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupNameErrors.Empty);
    }

    [Fact]
    public void Create_ConNombreSoloEspacios_RetornaError()
    {
        // Arrange
        const string nombre = "   ";

        // Act
        var result = GroupName.Create(nombre);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupNameErrors.Empty);
    }

    [Fact]
    public void Create_ConNombreMenorDe3Caracteres_RetornaError()
    {
        // Arrange
        const string nombre = "AB";

        // Act
        var result = GroupName.Create(nombre);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupNameErrors.TooShort);
    }

    [Fact]
    public void Create_ConNombreExactamenteDe3Caracteres_RetornaExito()
    {
        // Arrange
        const string nombre = "ABC";

        // Act
        var result = GroupName.Create(nombre);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ConNombreExactamenteDe100Caracteres_RetornaExito()
    {
        // Arrange
        var nombre = new string('A', 100);

        // Act
        var result = GroupName.Create(nombre);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ConNombreMayorDe100Caracteres_RetornaError()
    {
        // Arrange
        var nombre = new string('A', 101);

        // Act
        var result = GroupName.Create(nombre);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(GroupNameErrors.TooLong);
    }
}
