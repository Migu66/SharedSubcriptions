using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Application.Abstractions;
using Payments.Application.IntegrationEvents;
using Payments.Domain.Repositories;
using Payments.Infrastructure.EventHandlers;
using Payments.Infrastructure.Messaging;
using Payments.Infrastructure.Persistence;
using Payments.Infrastructure.Persistence.Interceptors;
using Payments.Infrastructure.Persistence.Repositories;
using Payments.Infrastructure.Services;
using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<DomainEventDispatcherInterceptor>();

        services.AddDbContext<PaymentsDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("PaymentsDb"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatcherInterceptor>());
        });

        services.AddScoped<IPaymentRecordRepository, PaymentRecordRepository>();
        services.AddScoped<IDebtRepository, DebtRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IStripePaymentService, StripePaymentService>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<BillingDueSoonIntegrationEventConsumer>();
            bus.AddConsumer<MemberAddedToGroupIntegrationEventConsumer>();

            bus.AddEntityFrameworkOutbox<PaymentsDbContext>(outbox =>
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
                cfg.Message<BillingDueSoonIntegrationEvent>(m =>
                    m.SetEntityName("Subscriptions.Application.IntegrationEvents.BillingDueSoonIntegrationEvent"));

                cfg.Message<MemberAddedToGroupIntegrationEvent>(m =>
                    m.SetEntityName("Groups.Application.IntegrationEvents.MemberAddedToGroupIntegrationEvent"));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
