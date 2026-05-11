using Analytics.Application.DTOs;
using Analytics.Domain.Errors;
using Analytics.Domain.Repositories;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Queries.GetGroupSavings;

internal sealed class GetGroupSavingsQueryHandler
    : IRequestHandler<GetGroupSavingsQuery, Result<GroupSavingsDto>>
{
    private readonly IGroupSavingsRepository _groupSavingsRepository;

    public GetGroupSavingsQueryHandler(IGroupSavingsRepository groupSavingsRepository)
    {
        _groupSavingsRepository = groupSavingsRepository;
    }

    public async Task<Result<GroupSavingsDto>> Handle(
        GetGroupSavingsQuery request,
        CancellationToken cancellationToken)
    {
        var readModel = await _groupSavingsRepository
            .GetByGroupIdAndYearAsync(request.GroupId, request.Year, cancellationToken);

        if (readModel is null)
            return Result.Success(new GroupSavingsDto(
                GroupId: request.GroupId.Value,
                Year: request.Year,
                TotalSpent: 0m,
                EstimatedSavings: 0m));

        return Result.Success(new GroupSavingsDto(
            GroupId: readModel.GroupId.Value,
            Year: readModel.Year,
            TotalSpent: readModel.TotalSpent,
            EstimatedSavings: readModel.EstimatedSavings));
    }
}
