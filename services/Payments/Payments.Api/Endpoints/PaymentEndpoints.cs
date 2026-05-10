using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Commands.ConfirmAdminPayment;
using Payments.Application.Commands.SettleDebt;
using Payments.Application.Commands.SettleDebtManually;
using Payments.Application.Queries.GetPaymentHistory;
using Payments.Application.Queries.GetPendingDebts;
using Payments.Domain.ValueObjects;

namespace Payments.Api.Endpoints;

public static class PaymentEndpoints
{
    public static IEndpointRouteBuilder MapPaymentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments")
            .RequireAuthorization();

        group.MapPost("/confirm", ConfirmAdminPaymentAsync)
            .WithName("ConfirmAdminPayment");

        group.MapPost("/debts/{debtId:guid}/settle", SettleDebtAsync)
            .WithName("SettleDebt");

        group.MapPost("/debts/{debtId:guid}/settle-manual", SettleDebtManuallyAsync)
            .WithName("SettleDebtManually");

        group.MapGet("/history/{subscriptionId:guid}", GetPaymentHistoryAsync)
            .WithName("GetPaymentHistory");

        group.MapGet("/debts/pending/{userId:guid}", GetPendingDebtsAsync)
            .WithName("GetPendingDebts");

        return app;
    }

    private static async Task<IResult> ConfirmAdminPaymentAsync(
        [FromBody] ConfirmAdminPaymentRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var subscriptionIdResult = SubscriptionId.From(request.SubscriptionId);
        if (subscriptionIdResult.IsFailure)
            return Results.BadRequest(subscriptionIdResult.Error);

        var groupIdResult = GroupId.From(request.GroupId);
        if (groupIdResult.IsFailure)
            return Results.BadRequest(groupIdResult.Error);

        var adminIdResult = UserId.From(request.AdminId);
        if (adminIdResult.IsFailure)
            return Results.BadRequest(adminIdResult.Error);

        var command = new ConfirmAdminPaymentCommand(
            subscriptionIdResult.Value,
            groupIdResult.Value,
            adminIdResult.Value,
            request.MemberIds,
            request.TotalAmount,
            request.Currency,
            request.PaidAt);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/payments/history/{request.SubscriptionId}", new { id = result.Value.Value })
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SettleDebtAsync(
        Guid debtId,
        [FromBody] SettleDebtRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var debtIdResult = DebtId.From(debtId);
        if (debtIdResult.IsFailure)
            return Results.BadRequest(debtIdResult.Error);

        var debtorIdResult = UserId.From(request.DebtorId);
        if (debtorIdResult.IsFailure)
            return Results.BadRequest(debtorIdResult.Error);

        var command = new SettleDebtCommand(debtIdResult.Value, debtorIdResult.Value);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> SettleDebtManuallyAsync(
        Guid debtId,
        [FromBody] SettleDebtManuallyRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var debtIdResult = DebtId.From(debtId);
        if (debtIdResult.IsFailure)
            return Results.BadRequest(debtIdResult.Error);

        var creditorIdResult = UserId.From(request.CreditorId);
        if (creditorIdResult.IsFailure)
            return Results.BadRequest(creditorIdResult.Error);

        var command = new SettleDebtManuallyCommand(debtIdResult.Value, creditorIdResult.Value);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Error);
    }

    private static async Task<IResult> GetPaymentHistoryAsync(
        Guid subscriptionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var idResult = SubscriptionId.From(subscriptionId);
        if (idResult.IsFailure)
            return Results.BadRequest(idResult.Error);

        var query = new GetPaymentHistoryQuery(idResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    private static async Task<IResult> GetPendingDebtsAsync(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var idResult = UserId.From(userId);
        if (idResult.IsFailure)
            return Results.BadRequest(idResult.Error);

        var query = new GetPendingDebtsQuery(idResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }
}

// Request DTOs (solo para la capa API, sin lógica)
public sealed record ConfirmAdminPaymentRequest(
    Guid SubscriptionId,
    Guid GroupId,
    Guid AdminId,
    IReadOnlyList<Guid> MemberIds,
    decimal TotalAmount,
    string Currency,
    DateTime PaidAt);

public sealed record SettleDebtRequest(Guid DebtorId);

public sealed record SettleDebtManuallyRequest(Guid CreditorId);
