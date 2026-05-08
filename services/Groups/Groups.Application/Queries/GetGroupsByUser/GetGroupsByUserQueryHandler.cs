using Groups.Domain.Repositories;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Queries.GetGroupsByUser;

// internal sealed: solo visible dentro de la capa Application, MediatR lo resuelve por DI.
// Las Queries no modifican estado, por eso no necesitan IUnitOfWork.
internal sealed class GetGroupsByUserQueryHandler
    : IRequestHandler<GetGroupsByUserQuery, Result<IReadOnlyList<GroupSummaryDto>>>
{
    private readonly IGroupRepository _groupRepository;

    public GetGroupsByUserQueryHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<Result<IReadOnlyList<GroupSummaryDto>>> Handle(
        GetGroupsByUserQuery request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Obtener todos los grupos en los que participa el usuario.
        // El repositorio filtra internamente los grupos donde el UserId aparece
        // en la colección de miembros.
        var groups = await _groupRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        // Paso 2: Proyectar cada grupo a su DTO de resumen.
        // Para cada grupo buscamos el miembro que coincide con el UserId pedido
        // y obtenemos su rol (Admin o Member), así el frontend puede mostrar
        // una etiqueta distinta según si es el dueño o solo participante.
        var summaries = groups
            .Select(g =>
            {
                var member = g.Members.First(m => m.Id == request.UserId);
                return new GroupSummaryDto(
                    Id: g.Id,
                    Name: g.Name.Value,
                    MemberCount: g.Members.Count,
                    UserRole: member.Role.ToString());
            })
            .ToList()
            .AsReadOnly();

        return Result.Success<IReadOnlyList<GroupSummaryDto>>(summaries);
    }
}
