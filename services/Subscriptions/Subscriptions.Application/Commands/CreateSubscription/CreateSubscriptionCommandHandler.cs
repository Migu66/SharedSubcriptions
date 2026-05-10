using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Aggregates;
using Subscriptions.Domain.Repositories;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Commands.CreateSubscription;

internal sealed class CreateSubscriptionCommandHandler
    : IRequestHandler<CreateSubscriptionCommand, Result<SubscriptionId>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<SubscriptionId>> Handle(
        CreateSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var moneyResult = Money.Create(request.TotalCost, request.Currency);
        if (moneyResult.IsFailure)
            return Result.Failure<SubscriptionId>(moneyResult.Error);

        var billingScheduleResult = BillingSchedule.Create(request.BillingCycle, request.FirstBillingDate);
        if (billingScheduleResult.IsFailure)
            return Result.Failure<SubscriptionId>(billingScheduleResult.Error);

        var subscriptionResult = Subscription.Create(
            request.GroupId,
            request.ServiceName,
            moneyResult.Value,
            billingScheduleResult.Value,
            _dateTimeProvider.UtcNow);

        if (subscriptionResult.IsFailure)
            return Result.Failure<SubscriptionId>(subscriptionResult.Error);

        var subscription = subscriptionResult.Value;

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(subscription.Id);
    }
}
