using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;

namespace ApiGateway.Tests;

public sealed class RoutingTests : IClassFixture<ApiGatewayWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RoutingTests(ApiGatewayWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData("/api/groups/test-id")]
    [InlineData("/api/subscriptions/test-id")]
    [InlineData("/api/payments/confirm")]
    [InlineData("/api/analytics/groups/test-id/savings")]
    [InlineData("/api/users/test-id/profile")]
    public async Task ProtectedRoute_IsKnownByGateway_Returns401NotFound(string path)
    {
        // Arrange
        // Sin token: el gateway conoce la ruta pero rechaza por falta de auth.
        // Si devuelve 404 significa que la ruta no está registrada en YARP.

        // Act
        var response = await _client.GetAsync(path);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            because: $"La ruta '{path}' debe estar registrada en YARP");
    }

    [Theory]
    [InlineData("/api/auth/login")]
    [InlineData("/api/auth/register")]
    [InlineData("/api/auth/refresh")]
    public async Task PublicRoute_IsKnownByGateway_DoesNotReturn404(string path)
    {
        // Arrange & Act
        var response = await _client.PostAsync(path, null);

        // Assert
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound,
            because: $"La ruta pública '{path}' debe estar registrada en YARP");
    }
}
