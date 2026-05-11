using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Application;
using Notifications.Application.Abstractions;
using Notifications.Application.IntegrationEvents;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Persistence.Repositories;
using Notifications.Infrastructure.Services;

namespace Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("NotificationsDb")));

        services.AddScoped<INotificationRecipientRepository, NotificationRecipientRepository>();

        services.AddScoped<IEmailSender, SendGridEmailSender>();
        services.AddScoped<IPushNotificationSender, FirebasePushNotificationSender>();
        services.AddScoped<ITelegramSender, TelegramBotSender>();
        services.AddHttpClient<IWhatsAppSender, WhatsAppBusinessSender>();

        services.AddMassTransit(bus =>
        {
            bus.AddApplicationConsumers();

            bus.AddEntityFrameworkOutbox<NotificationsDbContext>(outbox =>
            {
                outbox.UseSqlServer();
                outbox.UseBusOutbox();
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    configuration["RabbitMq:Host"]
                        ?? throw new InvalidOperationException("Falta la configuración 'RabbitMq:Host'."),
                    configuration["RabbitMq:VirtualHost"]
                        ?? throw new InvalidOperationException("Falta la configuración 'RabbitMq:VirtualHost'."),
                    h =>
                    {
                        h.Username(configuration["RabbitMq:Username"]
                            ?? throw new InvalidOperationException("Falta la configuración 'RabbitMq:Username'."));
                        h.Password(configuration["RabbitMq:Password"]
                            ?? throw new InvalidOperationException("Falta la configuración 'RabbitMq:Password'."));
                    });

                // Enlazamos los contratos locales a los exchanges publicados por otros servicios
                cfg.Message<BillingDueSoonIntegrationEvent>(m =>
                    m.SetEntityName("Subscriptions.Application.IntegrationEvents.BillingDueSoonIntegrationEvent"));

                cfg.Message<PaymentConfirmedIntegrationEvent>(m =>
                    m.SetEntityName("Payments.Application.IntegrationEvents.PaymentConfirmedIntegrationEvent"));

                cfg.Message<DebtSettledIntegrationEvent>(m =>
                    m.SetEntityName("Payments.Application.IntegrationEvents.DebtSettledIntegrationEvent"));

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
