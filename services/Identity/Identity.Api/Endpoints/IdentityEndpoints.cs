using Identity.Application.Commands.LoginUser;
using Identity.Application.Commands.RefreshToken;
using Identity.Application.Commands.RegisterUser;
using Identity.Application.Queries.GetUserProfile;
using Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        // Endpoints públicos de autenticación
        var auth = app.MapGroup("/api/auth");

        auth.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .AllowAnonymous();

        auth.MapPost("/login", LoginAsync)
            .WithName("Login")
            .AllowAnonymous();

        auth.MapPost("/refresh", RefreshTokenAsync)
            .WithName("RefreshToken")
            .AllowAnonymous();

        // Endpoints protegidos de usuario
        var users = app.MapGroup("/api/users")
            .RequireAuthorization();

        users.MapGet("/{userId:guid}/profile", GetUserProfileAsync)
            .WithName("GetUserProfile");

        return app;
    }

    // POST /api/auth/register
    private static async Task<IResult> RegisterAsync(
        [FromBody] RegisterUserRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName);

        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/users/{result.Value.Value}/profile", new { id = result.Value.Value })
            : Results.BadRequest(result.Error);
    }

    // POST /api/auth/login
    private static async Task<IResult> LoginAsync(
        [FromBody] LoginUserRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Unauthorized();
    }

    // POST /api/auth/refresh
    private static async Task<IResult> RefreshTokenAsync(
        [FromBody] RefreshTokenRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.Unauthorized();
    }

    // GET /api/users/{userId}/profile
    private static async Task<IResult> GetUserProfileAsync(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userIdResult = UserId.From(userId);
        if (userIdResult.IsFailure)
            return Results.BadRequest(userIdResult.Error);

        var query = new GetUserProfileQuery(userIdResult.Value);
        var result = await sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.NotFound(result.Error);
    }

    // Request types
    private sealed record RegisterUserRequest(string Email, string Password, string FirstName, string LastName);
    private sealed record LoginUserRequest(string Email, string Password);
    private sealed record RefreshTokenRequest(string RefreshToken);
}
