using Groups.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Commands.RemoveMember;

// Este record representa la intención de expulsar a un miembro de un grupo.
// AdminId es quien ejecuta la acción (debe ser el administrador del grupo).
// MemberToRemoveId es el UserId del miembro que se quiere eliminar.
public record RemoveMemberCommand(
    GroupId GroupId,
    UserId AdminId,
    UserId MemberToRemoveId) : IRequest<Result>;
