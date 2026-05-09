using FluentAssertions;
using Identity.Application.Commands.RegisterUser;
using Identity.Domain.Aggregates;
using Identity.Domain.Errors;
using Identity.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.Tests.Commands;

public class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly RegisterUserCommandHandler _handler;

    private static readonly DateTime UtcNow = new(2026, 5, 9, 10, 0, 0, DateTimeKind.Utc);

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(UtcNow);

        // UserManager requiere un IUserStore; el resto de dependencias son opcionales (null)
        var store = Substitute.For<IUserStore<ApplicationUser>>();
        _userManager = Substitute.For<UserManager<ApplicationUser>>(
            store,
            Substitute.For<IOptions<IdentityOptions>>(),
            Substitute.For<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Substitute.For<ILookupNormalizer>(),
            Substitute.For<IdentityErrorDescriber>(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<ApplicationUser>>>());

        _handler = new RegisterUserCommandHandler(
            _userRepository,
            _userManager,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_RegistroExitoso_RetornaUserId()
    {
        // Arrange
        var command = new RegisterUserCommand("juan@example.com", "Password1!", "Juan", "García");

        _userRepository.ExistsWithEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Success);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_EmailDuplicado_RetornaErrorEmailAlreadyExists()
    {
        // Arrange
        var command = new RegisterUserCommand("existente@example.com", "Password1!", "Juan", "García");

        _userRepository.ExistsWithEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.EmailAlreadyExists);
    }

    [Fact]
    public async Task Handle_EmailDuplicado_NoLlamaCreateAsync()
    {
        // Arrange
        var command = new RegisterUserCommand("existente@example.com", "Password1!", "Juan", "García");

        _userRepository.ExistsWithEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _userManager.DidNotReceive().CreateAsync(
            Arg.Any<ApplicationUser>(),
            Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_IdentityFalla_RetornaErrorDeIdentity()
    {
        // Arrange
        var command = new RegisterUserCommand("juan@example.com", "Password1!", "Juan", "García");

        _userRepository.ExistsWithEmailAsync(command.Email, Arg.Any<CancellationToken>())
            .Returns(false);

        var identityError = new IdentityError { Code = "PasswordTooShort", Description = "La contraseña es demasiado corta." };
        _userManager.CreateAsync(Arg.Any<ApplicationUser>(), command.Password)
            .Returns(IdentityResult.Failed(identityError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Identity.PasswordTooShort");
    }
}
