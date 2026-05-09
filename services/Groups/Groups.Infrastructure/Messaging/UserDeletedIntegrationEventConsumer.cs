using Groups.Application.Commands.RemoveUserFromAllGroups;
using Groups.Application.IntegrationEvents;
using Groups.Domain.ValueObjects;
using MassTransit;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Infrastructure.Messaging;

/// <summary>
/// Consume el evento UserDeletedIntegrationEvent publicado por Identity Service
/// y elimina al usuario de todos los grupos en los que es miembro.
/// La cola se enlaza al exchange de Identity configurando el entity name en DependencyInjection.
/// </summary>
internal sealed class UserDeletedIntegrationEventConsumer
    : IConsumer<UserDeletedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserDeletedIntegrationEventConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<UserDeletedIntegrationEvent> context)
    {
        var userIdResult = UserId.From(context.Message.UserId.Value);
        if (userIdResult.IsFailure)
            return; // GUID vacío; mensaje malformado, no hay nada que hacer.

        var command = new RemoveUserFromAllGroupsCommand(userIdResult.Value);

        await _mediator.Send(command, context.CancellationToken);
    }
}
