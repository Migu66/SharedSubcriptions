using Identity.Application.DTOs;
using Identity.Domain.Aggregates;

namespace Identity.Application.Abstractions;

public interface IJwtTokenService
{
    AuthTokenDto GenerateTokens(ApplicationUser user);
}
