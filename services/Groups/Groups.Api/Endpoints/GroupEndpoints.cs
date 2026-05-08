using Groups.Application.Commands.AddMember;
using Groups.Application.Commands.CreateGroup;
using Groups.Application.Commands.RemoveMember;
using Groups.Application.Queries.GetGroupDetails;
using Groups.Application.Queries.GetGroupsByUser;
using Groups.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Api.Endpoints;

public static class GroupEndpoints
{
    public static IEndpointRouteBuilder MapGroupEndpoints(this IEndpointRouteBuilder app)
    {
        var groups = app.MapGroup("/api/groups")
            .RequireAuthorization();

        groups.MapPost("/", CreateGroupAsync)
            .WithName("CreateGroup");

        groups.MapGet("/{groupId:guid}", GetGroupDetailsAsync)
            .WithName("GetGroupDetails");

        groups.MapGet("/user/{userId:guid}", GetGroupsByUserAsync)
            .WithName("GetGroupsByUser");

        groups.MapPost("/{groupId:guid}/members", AddMemberAsync)
            .WithName("AddMember");

        groups.MapDelete("/{groupId:guid}/members/{memberId:guid}", RemoveMemberAsync)
            .WithName("RemoveMember");

        return app;
    }

    // POST /api/groups
    private static async Task<IResult> CreateGroupAsync(
        [FromBody] CreateGroupRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var adminIdResult = UserId.From(request.AdminId);
        if (adminIdResult.IsFailure)
            return Results.BadRequest(adminIdResult.Error);

        var command = new CreateGroupCommand(request.Name, adminIdResult.Value, request.AdminEmail);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/groups/{result.Value.Value}", new { id = result.Value.Value })
            : Results.BadRequest(result.Error);
    }

    // GET /api/groups/{groupId}
    private static async Task<IResult> GetGroupDetailsAsync(
        Guid groupId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var groupIdResult = GroupId.From(groupId);
        if (groupIdResult.IsFailure)
            return Results.BadRequest(groupIdResult.Error);

        var query = new GetGroupDetailsQuery(groupIdResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    // GET /api/groups/user/{userId}
    private static async Task<IResult> GetGroupsByUserAsync(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.From(userId);
        if (userIdResult.IsFailure)
            return Results.BadRequest(userIdResult.Error);

        var query = new GetGroupsByUserQuery(userIdResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Error);
    }

    // POST /api/groups/{groupId}/members
    private static async Task<IResult> AddMemberAsync(
        Guid groupId,
        [FromBody] AddMemberRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var groupIdResult = GroupId.From(groupId);
        if (groupIdResult.IsFailure)
            return Results.BadRequest(groupIdResult.Error);

        var adminIdResult = UserId.From(request.AdminId);
        if (adminIdResult.IsFailure)
            return Results.BadRequest(adminIdResult.Error);

        var newMemberIdResult = UserId.From(request.NewMemberId);
        if (newMemberIdResult.IsFailure)
            return Results.BadRequest(newMemberIdResult.Error);

        var command = new AddMemberCommand(
            groupIdResult.Value,
            adminIdResult.Value,
            newMemberIdResult.Value,
            request.InviteeEmail);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Error);
    }

    // DELETE /api/groups/{groupId}/members/{memberId}
    private static async Task<IResult> RemoveMemberAsync(
        Guid groupId,
        Guid memberId,
        [FromQuery] Guid adminId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var groupIdResult = GroupId.From(groupId);
        if (groupIdResult.IsFailure)
            return Results.BadRequest(groupIdResult.Error);

        var adminIdResult = UserId.From(adminId);
        if (adminIdResult.IsFailure)
            return Results.BadRequest(adminIdResult.Error);

        var memberIdResult = UserId.From(memberId);
        if (memberIdResult.IsFailure)
            return Results.BadRequest(memberIdResult.Error);

        var command = new RemoveMemberCommand(
            groupIdResult.Value,
            adminIdResult.Value,
            memberIdResult.Value);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.NoContent()
            : Results.BadRequest(result.Error);
    }
}

// Request bodies
internal sealed record CreateGroupRequest(string Name, Guid AdminId, string AdminEmail);
internal sealed record AddMemberRequest(Guid AdminId, Guid NewMemberId, string InviteeEmail);
