using Groups.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Commands.AddMember;

// Este record representa la intención de añadir un nuevo miembro a un grupo existente.
// AdminId es quien ejecuta la acción (debe ser el administrador del grupo).
// NewMemberId es el UserId del usuario que se va a incorporar.
// InviteeEmail es el email que se mostrará al nuevo miembro dentro del grupo.
public record AddMemberCommand(
    GroupId GroupId,
    UserId AdminId,
    UserId NewMemberId,
    string InviteeEmail) : IRequest<Result>;
