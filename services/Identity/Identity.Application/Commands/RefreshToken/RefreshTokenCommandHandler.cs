using Identity.Application.Abstractions;
using Identity.Application.DTOs;
using Identity.Domain.Aggregates;
using Identity.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedSubscriptions.SharedKernel.Domain;
using RefreshTokenEntity = Identity.Domain.Entities.RefreshToken;

namespace Identity.Application.Commands.RefreshToken;

internal sealed class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    private static readonly Error InvalidToken = new(
        "RefreshToken.Invalid",
        "El token de refresco no es válido o ha expirado.");

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<AuthTokenDto>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Buscar el refresh token en base de datos.
        var stored = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (stored is null)
            return Result.Failure<AuthTokenDto>(InvalidToken);

        // Paso 2: Verificar que el token sigue activo (no revocado ni expirado).
        if (!stored.IsActive(_dateTimeProvider.UtcNow))
            return Result.Failure<AuthTokenDto>(InvalidToken);

        // Paso 3: Cargar el usuario asociado al token.
        var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
            return Result.Failure<AuthTokenDto>(InvalidToken);

        // Paso 4: Revocar el token usado (rotación de refresh tokens).
        stored.Revoke();

        // Paso 5: Generar un nuevo par de tokens.
        var newTokens = _jwtTokenService.GenerateTokens(user);

        // Paso 6: Persistir el nuevo refresh token.
        var newRefreshToken = RefreshTokenEntity.Create(
            userId: user.Id,
            token: newTokens.RefreshToken,
            expiresAt: newTokens.ExpiresAt.AddDays(7),
            createdAt: _dateTimeProvider.UtcNow);

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(newTokens);
    }
}
