using Groups.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Commands.CreateGroup;

// Este record es el "formulario" que se envía al sistema cuando alguien quiere crear un grupo.
// Implementa IRequest<Result<GroupId>> para que MediatR sepa que este comando
// debe devolver un Result que contiene el ID del grupo recién creado.
public record CreateGroupCommand(
    string Name,
    UserId AdminId,
    string AdminEmail) : IRequest<Result<GroupId>>;
