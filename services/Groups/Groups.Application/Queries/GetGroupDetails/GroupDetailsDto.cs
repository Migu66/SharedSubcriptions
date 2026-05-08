using Groups.Domain.ValueObjects;

namespace Groups.Application.Queries.GetGroupDetails;

// DTO con todos los datos del grupo que se devuelven al cliente.
// Incluye la lista completa de miembros ya proyectados a MemberDto.
public sealed record GroupDetailsDto(
    GroupId Id,
    string Name,
    UserId AdminId,
    DateTime CreatedAt,
    IReadOnlyList<MemberDto> Members);
