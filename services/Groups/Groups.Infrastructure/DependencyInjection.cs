using Groups.Domain.Repositories;
using Groups.Infrastructure.Persistence;
using Groups.Infrastructure.Persistence.Interceptors;
using Groups.Infrastructure.Persistence.Repositories;
using Groups.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // El interceptor necesita ser Scoped porque IPublisher (MediatR) es Scoped
        services.AddScoped<DomainEventDispatcherInterceptor>();

        services.AddDbContext<GroupsDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("GroupsDb"));
            options.AddInterceptors(sp.GetRequiredService<DomainEventDispatcherInterceptor>());
        });

        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IInvitationRepository, InvitationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddMassTransit(bus =>
        {
            // Outbox transaccional: los eventos se guardan en la misma BD
            // antes de enviarse a RabbitMQ, garantizando que nunca se pierdan
            bus.AddEntityFrameworkOutbox<GroupsDbContext>(outbox =>
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

                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
