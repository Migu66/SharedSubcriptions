using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedSubscriptions.SharedKernel.Domain;
using Subscriptions.Application.IntegrationEvents;
using Subscriptions.Domain.Repositories;
using Subscriptions.Infrastructure.EventHandlers;
using Subscriptions.Infrastructure.Messaging;
using Subscriptions.Infrastructure.Persistence;
using Subscriptions.Infrastructure.Persistence.Interceptors;
using Subscriptions.Infrastructure.Persistence.Repositories;
using Subscriptions.Infrastructure.Services;

namespace Subscriptions.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DomainEventDispatcherInterceptor>();

        services.AddDbContext<SubscriptionsDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SubscriptionsDb"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatcherInterceptor>());
        });

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<MemberRemovedFromGroupIntegrationEventConsumer>();

            bus.AddEntityFrameworkOutbox<SubscriptionsDbContext>(outbox =>
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

                // Enlazamos el contrato local al exchange publicado por Groups Service
                cfg.Message<MemberRemovedFromGroupIntegrationEvent>(m =>
                    m.SetEntityName("Groups.Application.IntegrationEvents.MemberRemovedFromGroupIntegrationEvent"));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
