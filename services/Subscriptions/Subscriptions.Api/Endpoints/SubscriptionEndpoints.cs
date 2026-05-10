using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscriptions.Application.Commands.CreateSubscription;
using Subscriptions.Application.Commands.UpdateSubscriptionPrice;
using Subscriptions.Application.Queries.GetSubscriptionDetails;
using Subscriptions.Application.Queries.GetSubscriptionsByGroup;
using Subscriptions.Domain.Enums;
using Subscriptions.Domain.ValueObjects;

namespace Subscriptions.Api.Endpoints;

public static class SubscriptionEndpoints
{
    public static IEndpointRouteBuilder MapSubscriptionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subscriptions")
            .RequireAuthorization();

        group.MapPost("/", CreateSubscriptionAsync)
            .WithName("CreateSubscription");

        group.MapGet("/{subscriptionId:guid}", GetSubscriptionDetailsAsync)
            .WithName("GetSubscriptionDetails");

        group.MapGet("/group/{groupId:guid}", GetSubscriptionsByGroupAsync)
            .WithName("GetSubscriptionsByGroup");

        group.MapPut("/{subscriptionId:guid}/price", UpdateSubscriptionPriceAsync)
            .WithName("UpdateSubscriptionPrice");

        return app;
    }

    private static async Task<IResult> CreateSubscriptionAsync(
        [FromBody] CreateSubscriptionRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var groupIdResult = GroupId.From(request.GroupId);
        if (groupIdResult.IsFailure)
            return Results.BadRequest(groupIdResult.Error);

        var adminIdResult = UserId.From(request.AdminId);
        if (adminIdResult.IsFailure)
            return Results.BadRequest(adminIdResult.Error);

        var command = new CreateSubscriptionCommand(
            groupIdResult.Value,
            adminIdResult.Value,
            request.ServiceName,
            request.TotalCost,
            request.Currency,
            request.BillingCycle,
            request.FirstBillingDate);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/subscriptions/{result.Value.Value}", new { id = result.Value.Value })
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetSubscriptionDetailsAsync(
        Guid subscriptionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var idResult = SubscriptionId.From(subscriptionId);
        if (idResult.IsFailure)
            return Results.BadRequest(idResult.Error);

        var query = new GetSubscriptionDetailsQuery(idResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> GetSubscriptionsByGroupAsync(
        Guid groupId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var idResult = GroupId.From(groupId);
        if (idResult.IsFailure)
            return Results.BadRequest(idResult.Error);

        var query = new GetSubscriptionsByGroupQuery(idResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> UpdateSubscriptionPriceAsync(
        Guid subscriptionId,
        [FromBody] UpdateSubscriptionPriceRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var idResult = SubscriptionId.From(subscriptionId);
        if (idResult.IsFailure)
            return Results.BadRequest(idResult.Error);

        var adminIdResult = UserId.From(request.AdminId);
        if (adminIdResult.IsFailure)
            return Results.BadRequest(adminIdResult.Error);

        var command = new UpdateSubscriptionPriceCommand(
            idResult.Value,
            adminIdResult.Value,
            request.NewAmount,
            request.Currency);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Error);
    }
}

public sealed record CreateSubscriptionRequest(
    Guid GroupId,
    Guid AdminId,
    string ServiceName,
    decimal TotalCost,
    string Currency,
    BillingCycle BillingCycle,
    DateTime FirstBillingDate);

public sealed record UpdateSubscriptionPriceRequest(
    Guid AdminId,
    decimal NewAmount,
    string Currency);
