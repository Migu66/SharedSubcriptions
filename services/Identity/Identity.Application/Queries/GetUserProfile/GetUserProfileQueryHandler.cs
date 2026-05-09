using Identity.Domain.Errors;
using Identity.Domain.Repositories;
using Identity.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.Queries.GetUserProfile;

internal sealed class GetUserProfileQueryHandler
    : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserProfileDto>> Handle(
        GetUserProfileQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<UserProfileDto>(UserErrors.NotFound);

        var userId = UserId.From(user.Id);
        if (userId.IsFailure)
            return Result.Failure<UserProfileDto>(userId.Error);

        var dto = new UserProfileDto(
            Id: userId.Value,
            Email: user.Email!,
            FirstName: user.FirstName,
            LastName: user.LastName,
            CreatedAt: user.CreatedAt);

        return Result.Success(dto);
    }
}
