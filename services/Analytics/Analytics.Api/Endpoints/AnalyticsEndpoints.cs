using Analytics.Application.Queries.GetDebtHistory;
using Analytics.Application.Queries.GetGroupSavings;
using Analytics.Application.Queries.GetServiceSpending;
using Analytics.Domain.ValueObjects;
using MediatR;

namespace Analytics.Api.Endpoints;

public static class AnalyticsEndpoints
{
    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/analytics")
            .RequireAuthorization();

        group.MapGet("/groups/{groupId:guid}/savings", GetGroupSavingsAsync)
            .WithName("GetGroupSavings");

        group.MapGet("/groups/{groupId:guid}/spending", GetServiceSpendingAsync)
            .WithName("GetServiceSpending");

        group.MapGet("/users/{userId:guid}/debts", GetDebtHistoryAsync)
            .WithName("GetDebtHistory");

        return app;
    }

    private static async Task<IResult> GetGroupSavingsAsync(
        Guid groupId,
        int year,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var groupIdResult = GroupId.From(groupId);
        if (groupIdResult.IsFailure)
            return Results.BadRequest(groupIdResult.Error);

        var query = new GetGroupSavingsQuery(groupIdResult.Value, year);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetServiceSpendingAsync(
        Guid groupId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var groupIdResult = GroupId.From(groupId);
        if (groupIdResult.IsFailure)
            return Results.BadRequest(groupIdResult.Error);

        var query = new GetServiceSpendingQuery(groupIdResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetDebtHistoryAsync(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.From(userId);
        if (userIdResult.IsFailure)
            return Results.BadRequest(userIdResult.Error);

        var query = new GetDebtHistoryQuery(userIdResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }
}
