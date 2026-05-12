using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace ApiGateway.Tests;

public sealed class ApiGatewayWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtSettings:SecretKey"] = "test-secret-key-for-integration-tests-min32chars",
                ["JwtSettings:Issuer"] = "SharedSubscriptions",
                ["JwtSettings:Audience"] = "SharedSubscriptions.Api",
                // Clusters apuntan a destinos ficticios para tests de enrutamiento
                ["ReverseProxy:Clusters:identity-cluster:Destinations:destination1:Address"] = "http://localhost:9999",
                ["ReverseProxy:Clusters:groups-cluster:Destinations:destination1:Address"] = "http://localhost:9999",
                ["ReverseProxy:Clusters:subscriptions-cluster:Destinations:destination1:Address"] = "http://localhost:9999",
                ["ReverseProxy:Clusters:payments-cluster:Destinations:destination1:Address"] = "http://localhost:9999",
                ["ReverseProxy:Clusters:analytics-cluster:Destinations:destination1:Address"] = "http://localhost:9999"
            });
        });
    }
}
