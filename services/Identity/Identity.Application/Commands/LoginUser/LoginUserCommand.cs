using Identity.Application.DTOs;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.Commands.LoginUser;

public record LoginUserCommand(
    string Email,
    string Password) : IRequest<Result<AuthTokenDto>>;
