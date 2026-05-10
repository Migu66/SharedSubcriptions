using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Errors;
using Subscriptions.Domain.Repositories;

namespace Subscriptions.Application.Queries.GetSubscriptionDetails;

internal sealed class GetSubscriptionDetailsQueryHandler
    : IRequestHandler<GetSubscriptionDetailsQuery, Result<SubscriptionDetailsDto>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetSubscriptionDetailsQueryHandler(ISubscriptionRepository subscriptionRepository)
    {
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<SubscriptionDetailsDto>> Handle(
        GetSubscriptionDetailsQuery request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(
            request.SubscriptionId, cancellationToken);

        if (subscription is null)
            return Result.Failure<SubscriptionDetailsDto>(SubscriptionErrors.NotFound);

        var dto = new SubscriptionDetailsDto(
            Id: subscription.Id,
            GroupId: subscription.GroupId,
            ServiceName: subscription.ServiceName,
            TotalCost: subscription.TotalCost.Amount,
            Currency: subscription.TotalCost.Currency,
            BillingCycle: subscription.BillingSchedule.Cycle,
            NextBillingDate: subscription.BillingSchedule.NextBillingDate,
            CreatedAt: subscription.CreatedAt,
            IsActive: subscription.IsActive);

        return Result.Success(dto);
    }
}
