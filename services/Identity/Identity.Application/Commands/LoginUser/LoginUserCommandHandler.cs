using Identity.Application.Abstractions;
using Identity.Application.DTOs;
using Identity.Domain.Aggregates;
using Identity.Domain.Errors;
using Identity.Domain.Entities;
using Identity.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.Commands.LoginUser;

internal sealed class LoginUserCommandHandler
    : IRequestHandler<LoginUserCommand, Result<AuthTokenDto>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<AuthTokenDto>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Buscar el usuario por email.
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure<AuthTokenDto>(UserErrors.NotFound);

        // Paso 2: Verificar la contraseña sin crear una cookie de sesión (CheckPasswordSignInAsync).
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
            return Result.Failure<AuthTokenDto>(new Error("User.InvalidCredentials", "Las credenciales proporcionadas no son correctas."));

        // Paso 3: Generar el access token y el refresh token.
        var tokens = _jwtTokenService.GenerateTokens(user);

        // Paso 4: Persistir el refresh token en base de datos.
        var refreshToken = RefreshToken.Create(
            userId: user.Id,
            token: tokens.RefreshToken,
            expiresAt: tokens.ExpiresAt.AddDays(7),
            createdAt: _dateTimeProvider.UtcNow);

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tokens);
    }
}
