using Analytics.Application.Consumers;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }

    /// <summary>
    /// Registra los consumidores de MassTransit definidos en Application.
    /// Se llama desde la configuración de MassTransit en Infrastructure.
    /// </summary>
    public static void AddApplicationConsumers(this IBusRegistrationConfigurator cfg)
    {
        cfg.AddConsumer<PaymentConfirmedIntegrationEventConsumer>();
        cfg.AddConsumer<DebtCreatedIntegrationEventConsumer>();
        cfg.AddConsumer<DebtSettledIntegrationEventConsumer>();
        cfg.AddConsumer<SubscriptionCreatedIntegrationEventConsumer>();
        cfg.AddConsumer<MemberAddedToGroupIntegrationEventConsumer>();
    }
}
