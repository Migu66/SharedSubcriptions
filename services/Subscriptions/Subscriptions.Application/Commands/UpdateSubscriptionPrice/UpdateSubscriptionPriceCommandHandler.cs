using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.Errors;
using Subscriptions.Domain.Repositories;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Commands.UpdateSubscriptionPrice;

internal sealed class UpdateSubscriptionPriceCommandHandler
    : IRequestHandler<UpdateSubscriptionPriceCommand, Result>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UpdateSubscriptionPriceCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(
        UpdateSubscriptionPriceCommand request,
        CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetByIdAsync(
            request.SubscriptionId, cancellationToken);

        if (subscription is null)
            return Result.Failure(SubscriptionErrors.NotFound);

        var moneyResult = Money.Create(request.NewAmount, request.Currency);
        if (moneyResult.IsFailure)
            return Result.Failure(moneyResult.Error);

        var updateResult = subscription.UpdatePrice(moneyResult.Value, _dateTimeProvider.UtcNow);
        if (updateResult.IsFailure)
            return updateResult;

        _subscriptionRepository.Update(subscription);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
