using Analytics.Application;
using Analytics.Application.IntegrationEvents;
using Analytics.Domain.Repositories;
using Analytics.Infrastructure.Persistence;
using Analytics.Infrastructure.Persistence.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("AnalyticsDb")
                ?? throw new InvalidOperationException("Falta la cadena de conexión 'AnalyticsDb'.")));

        services.AddScoped<IGroupSavingsRepository, GroupSavingsRepository>();
        services.AddScoped<IServiceSpendingRepository, ServiceSpendingRepository>();
        services.AddScoped<IDebtHistoryRepository, DebtHistoryRepository>();
        services.AddScoped<ISubscriptionContextRepository, SubscriptionContextRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMassTransit(bus =>
        {
            bus.AddApplicationConsumers();

            bus.AddEntityFrameworkOutbox<AnalyticsDbContext>(outbox =>
            {
                outbox.UseSqlServer();
                outbox.UseBusOutbox();
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    configuration["RabbitMq:Host"] ?? throw new InvalidOperationException("Falta la configuración 'RabbitMq:Host'."),
                    configuration["RabbitMq:VirtualHost"] ?? throw new InvalidOperationException("Falta la configuración 'RabbitMq:VirtualHost'."),
                    h =>
                    {
                        h.Username(configuration["RabbitMq:Username"] ?? throw new InvalidOperationException("Falta la configuración 'RabbitMq:Username'."));
                        h.Password(configuration["RabbitMq:Password"] ?? throw new InvalidOperationException("Falta la configuración 'RabbitMq:Password'."));
                    });

                // Enlazamos los contratos locales a los exchanges publicados por otros servicios
                cfg.Message<PaymentConfirmedIntegrationEvent>(m =>
                    m.SetEntityName("Payments.Application.IntegrationEvents.PaymentConfirmedIntegrationEvent"));

                cfg.Message<DebtCreatedIntegrationEvent>(m =>
                    m.SetEntityName("Payments.Application.IntegrationEvents.DebtCreatedIntegrationEvent"));

                cfg.Message<DebtSettledIntegrationEvent>(m =>
                    m.SetEntityName("Payments.Application.IntegrationEvents.DebtSettledIntegrationEvent"));

                cfg.Message<SubscriptionCreatedIntegrationEvent>(m =>
                    m.SetEntityName("Subscriptions.Application.IntegrationEvents.SubscriptionCreatedIntegrationEvent"));

                cfg.Message<MemberAddedToGroupIntegrationEvent>(m =>
                    m.SetEntityName("Groups.Application.IntegrationEvents.MemberAddedToGroupIntegrationEvent"));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
