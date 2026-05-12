using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace ApiGateway.Tests;

public sealed class AuthenticationTests : IClassFixture<ApiGatewayWebApplicationFactory>
{
    private const string SecretKey = "test-secret-key-for-integration-tests-min32chars";
    private const string Issuer = "SharedSubscriptions";
    private const string Audience = "SharedSubscriptions.Api";

    private readonly HttpClient _client;

    public AuthenticationTests(ApiGatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData("/api/groups/any-id")]
    [InlineData("/api/subscriptions/any-id")]
    [InlineData("/api/payments/confirm")]
    [InlineData("/api/analytics/groups/any-id/savings")]
    [InlineData("/api/users/any-id/profile")]
    public async Task ProtectedEndpoint_WithoutToken_Returns401(string path)
    {
        // Arrange
        // (sin cabecera de autorización)

        // Act
        var response = await _client.GetAsync(path);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/refresh")]
    public async Task PublicAuthEndpoint_WithoutToken_DoesNotReturn401(string path)
    {
        // Arrange
        // (sin cabecera de autorización)

        // Act
        var response = await _client.PostAsync(path, null);

        // Assert
        // El gateway deja pasar la petición (no devuelve 401).
        // Puede devolver 502/503 porque el microservicio no existe en tests, pero no 401.
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_DoesNotReturn401()
    {
        // Arrange
        var token = GenerateJwtToken(userId: Guid.NewGuid().ToString());
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/groups/some-id");

        // Assert
        // Con token válido el gateway deja pasar (puede ser 502 porque el servicio no existe, pero no 401)
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithExpiredToken_Returns401()
    {
        // Arrange
        var token = GenerateJwtToken(userId: Guid.NewGuid().ToString(), expiredMinutesAgo: 10);
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/groups/some-id");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static string GenerateJwtToken(string userId, int expiredMinutesAgo = 0)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiry = expiredMinutesAgo > 0
            ? DateTime.UtcNow.AddMinutes(-expiredMinutesAgo)
            : DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: [new Claim(JwtRegisteredClaimNames.Sub, userId)],
            notBefore: expiredMinutesAgo > 0 ? DateTime.UtcNow.AddHours(-2) : DateTime.UtcNow,
            expires: expiry,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
