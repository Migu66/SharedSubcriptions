using Groups.Domain.ValueObjects;

namespace Groups.Application.Queries.GetGroupsByUser;

// DTO que representa el resumen de un grupo en el listado del usuario.
// Es más ligero que GroupDetailsDto: no incluye la lista completa de miembros,
// solo un contador y el rol que tiene ese usuario concreto en el grupo.
public sealed record GroupSummaryDto(
    GroupId Id,
    string Name,
    int MemberCount,
    string UserRole);
