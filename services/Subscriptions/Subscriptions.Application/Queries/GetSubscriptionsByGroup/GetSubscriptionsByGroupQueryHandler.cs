using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Repositories;

namespace Subscriptions.Application.Queries.GetSubscriptionsByGroup;

internal sealed class GetSubscriptionsByGroupQueryHandler
    : IRequestHandler<GetSubscriptionsByGroupQuery, Result<IReadOnlyList<SubscriptionSummaryDto>>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionsByGroupQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<IReadOnlyList<SubscriptionSummaryDto>>> Handle(
        GetSubscriptionsByGroupQuery request,
        CancellationToken cancellationToken)
    {
        var subscriptions = await _subscriptionRepository.GetByGroupIdAsync(
            request.GroupId, cancellationToken);

        var dtos = subscriptions
            .Select(s => new SubscriptionSummaryDto(
                Id: s.Id,
                ServiceName: s.ServiceName,
                TotalCost: s.TotalCost.Amount,
                Currency: s.TotalCost.Currency,
                BillingCycle: s.BillingSchedule.Cycle,
                NextBillingDate: s.BillingSchedule.NextBillingDate,
                IsActive: s.IsActive))
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<SubscriptionSummaryDto>>(dtos);
    }
}
