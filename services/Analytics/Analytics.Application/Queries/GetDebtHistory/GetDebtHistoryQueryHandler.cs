using Analytics.Application.DTOs;
using Analytics.Domain.Repositories;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Queries.GetDebtHistory;

internal sealed class GetDebtHistoryQueryHandler
    : IRequestHandler<GetDebtHistoryQuery, Result<DebtHistoryDto>>
{
    private readonly IDebtHistoryRepository _debtHistoryRepository;

    public GetDebtHistoryQueryHandler(IDebtHistoryRepository debtHistoryRepository)
    {
        _debtHistoryRepository = debtHistoryRepository;
    }

    public async Task<Result<DebtHistoryDto>> Handle(
        GetDebtHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var readModel = await _debtHistoryRepository
            .GetByUserIdAsync(request.UserId, cancellationToken);

        if (readModel is null)
            return Result.Success(new DebtHistoryDto(
                UserId: request.UserId.Value,
                TotalDebt: 0m,
                TotalSettled: 0m,
                PendingCount: 0));

        return Result.Success(new DebtHistoryDto(
            UserId: readModel.UserId.Value,
            TotalDebt: readModel.TotalDebt,
            TotalSettled: readModel.TotalSettled,
            PendingCount: readModel.PendingCount));
    }
}
