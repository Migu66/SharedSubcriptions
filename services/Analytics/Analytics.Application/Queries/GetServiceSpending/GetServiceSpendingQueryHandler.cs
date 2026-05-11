using Analytics.Application.DTOs;
using Analytics.Domain.Repositories;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Application.Queries.GetServiceSpending;

internal sealed class GetServiceSpendingQueryHandler
    : IRequestHandler<GetServiceSpendingQuery, Result<IReadOnlyList<ServiceSpendingDto>>>
{
    private readonly IServiceSpendingRepository _serviceSpendingRepository;

    public GetServiceSpendingQueryHandler(IServiceSpendingRepository serviceSpendingRepository)
    {
        _serviceSpendingRepository = serviceSpendingRepository;
    }

    public async Task<Result<IReadOnlyList<ServiceSpendingDto>>> Handle(
        GetServiceSpendingQuery request,
        CancellationToken cancellationToken)
    {
        var readModels = await _serviceSpendingRepository
            .GetByGroupIdAsync(request.GroupId, cancellationToken);

        var dtos = readModels
            .Select(m => new ServiceSpendingDto(
                GroupId: m.GroupId.Value,
                ServiceName: m.ServiceName,
                TotalSpent: m.TotalSpent,
                PaymentCount: m.PaymentCount))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<ServiceSpendingDto>>(dtos);
    }
}
