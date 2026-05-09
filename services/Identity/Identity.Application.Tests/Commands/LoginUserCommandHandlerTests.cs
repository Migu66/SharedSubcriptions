using FluentAssertions;
using Identity.Application.Abstractions;
using Identity.Application.Commands.LoginUser;
using Identity.Application.DTOs;
using Identity.Domain.Aggregates;
using Identity.Domain.Errors;
using Identity.Domain.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.Tests.Commands;

public class LoginUserCommandHandlerTests
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly LoginUserCommandHandler _handler;

    private static readonly DateTime UtcNow = new(2026, 5, 9, 10, 0, 0, DateTimeKind.Utc);

    public LoginUserCommandHandlerTests()
    {
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(UtcNow);

        _jwtTokenService = Substitute.For<IJwtTokenService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        // Construir UserManager con todos los substitutos requeridos
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

        // Construir SignInManager
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        var claimsFactory = Substitute.For<IUserClaimsPrincipalFactory<ApplicationUser>>();
        _signInManager = Substitute.For<SignInManager<ApplicationUser>>(
            _userManager,
            contextAccessor,
            claimsFactory,
            Substitute.For<IOptions<IdentityOptions>>(),
            Substitute.For<ILogger<SignInManager<ApplicationUser>>>(),
            Substitute.For<IAuthenticationSchemeProvider>(),
            Substitute.For<IUserConfirmation<ApplicationUser>>());

        _handler = new LoginUserCommandHandler(
            _userManager,
            _signInManager,
            _jwtTokenService,
            _refreshTokenRepository,
            _unitOfWork,
            _dateTimeProvider);
    }

    [Fact]
    public async Task Handle_CredencialesValidas_RetornaTokens()
    {
        // Arrange
        var user = ApplicationUser.Create("juan@example.com", "Juan", "García", UtcNow).Value;
        var command = new LoginUserCommand("juan@example.com", "Password1!");
        var tokensDto = new AuthTokenDto("access-token", "refresh-token", UtcNow.AddHours(1));

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, false)
            .Returns(SignInResult.Success);
        _jwtTokenService.GenerateTokens(user).Returns(tokensDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_RetornaErrorNotFound()
    {
        // Arrange
        var command = new LoginUserCommand("noexiste@example.com", "Password1!");

        _userManager.FindByEmailAsync(command.Email).ReturnsNull();

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ContrasenaIncorrecta_RetornaErrorCredenciales()
    {
        // Arrange
        var user = ApplicationUser.Create("juan@example.com", "Juan", "García", UtcNow).Value;
        var command = new LoginUserCommand("juan@example.com", "WrongPassword!");

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, false)
            .Returns(SignInResult.Failed);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_LoginExitoso_PersististeRefreshToken()
    {
        // Arrange
        var user = ApplicationUser.Create("juan@example.com", "Juan", "García", UtcNow).Value;
        var command = new LoginUserCommand("juan@example.com", "Password1!");
        var tokensDto = new AuthTokenDto("access-token", "refresh-token", UtcNow.AddHours(1));

        _userManager.FindByEmailAsync(command.Email).Returns(user);
        _signInManager.CheckPasswordSignInAsync(user, command.Password, false)
            .Returns(SignInResult.Success);
        _jwtTokenService.GenerateTokens(user).Returns(tokensDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _refreshTokenRepository.Received(1).AddAsync(
            Arg.Any<Identity.Domain.Entities.RefreshToken>(),
            Arg.Any<CancellationToken>());

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
