using MediatR;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Application.Commands.UpdateSubscriptionPrice;

public record UpdateSubscriptionPriceCommand(
    SubscriptionId SubscriptionId,
    UserId AdminId,
    decimal NewAmount,
    string Currency) : IRequest<Result>;
