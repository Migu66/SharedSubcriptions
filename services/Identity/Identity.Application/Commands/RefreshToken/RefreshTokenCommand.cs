using Identity.Application.DTOs;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.Commands.RefreshToken;

public record RefreshTokenCommand(
    string RefreshToken) : IRequest<Result<AuthTokenDto>>;
