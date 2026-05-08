using Groups.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Queries.GetGroupDetails;

// Este record es el "ticket de consulta" que se envía al sistema cuando alguien
// quiere ver los detalles de un grupo concreto.
// Las Queries nunca modifican datos, solo leen y devuelven información.
public record GetGroupDetailsQuery(GroupId GroupId) : IRequest<Result<GroupDetailsDto>>;
