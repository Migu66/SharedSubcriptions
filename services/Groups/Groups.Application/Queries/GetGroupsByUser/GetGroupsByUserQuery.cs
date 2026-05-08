using Groups.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Queries.GetGroupsByUser;

// "Ticket de consulta" que pide todos los grupos a los que pertenece un usuario.
// Devuelve una lista de resúmenes, no el detalle completo de cada grupo.
public record GetGroupsByUserQuery(UserId UserId) : IRequest<Result<IReadOnlyList<GroupSummaryDto>>>;
