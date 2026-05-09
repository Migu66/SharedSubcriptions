using Identity.Domain.ValueObjects;

namespace Identity.Application.Queries.GetUserProfile;

public sealed record UserProfileDto(
    UserId Id,
    string Email,
    string FirstName,
    string LastName,
    DateTime CreatedAt);
