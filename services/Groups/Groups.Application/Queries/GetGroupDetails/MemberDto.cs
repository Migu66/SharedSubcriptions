using Groups.Domain.ValueObjects;

namespace Groups.Application.Queries.GetGroupDetails;

// DTO (Data Transfer Object): objeto plano de solo lectura que representa
// a un miembro del grupo tal como lo recibe el cliente (web, móvil o API).
// No tiene lógica de negocio, solo datos.
public sealed record MemberDto(
    UserId Id,
    string Email,
    string Role,
    DateTime JoinedAt);
