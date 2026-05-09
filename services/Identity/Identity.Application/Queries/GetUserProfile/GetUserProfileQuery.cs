using Identity.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.Queries.GetUserProfile;

public record GetUserProfileQuery(UserId UserId) : IRequest<Result<UserProfileDto>>;
