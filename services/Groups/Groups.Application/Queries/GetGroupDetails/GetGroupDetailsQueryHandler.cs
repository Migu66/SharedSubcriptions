using Groups.Domain.Errors;
using Groups.Domain.Repositories;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Queries.GetGroupDetails;

// internal sealed: solo visible dentro de la capa Application, MediatR lo resuelve por DI.
// Las Queries no modifican estado, por eso no necesitan IUnitOfWork.
internal sealed class GetGroupDetailsQueryHandler
    : IRequestHandler<GetGroupDetailsQuery, Result<GroupDetailsDto>>
{
    private readonly IGroupRepository _groupRepository;

    public GetGroupDetailsQueryHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<Result<GroupDetailsDto>> Handle(
        GetGroupDetailsQuery request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Buscar el grupo en la base de datos por su ID.
        // Si no existe, devolvemos un error. No lanzamos excepciones.
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
            return Result.Failure<GroupDetailsDto>(GroupErrors.NotFound);

        // Paso 2: Proyectar cada miembro del agregado a su DTO correspondiente.
        // Convertimos el enum GroupRole a string para que el cliente reciba
        // "Admin" o "Member" en lugar de un número entero.
        var memberDtos = group.Members
            .Select(m => new MemberDto(
                Id: m.Id,
                Email: m.Email,
                Role: m.Role.ToString(),
                JoinedAt: m.JoinedAt))
            .ToList()
            .AsReadOnly();

        // Paso 3: Construir el DTO raíz con todos los datos del grupo y devolverlo.
        var dto = new GroupDetailsDto(
            Id: group.Id,
            Name: group.Name.Value,
            AdminId: group.AdminId,
            CreatedAt: group.CreatedAt,
            Members: memberDtos);

        return Result.Success(dto);
    }
}
