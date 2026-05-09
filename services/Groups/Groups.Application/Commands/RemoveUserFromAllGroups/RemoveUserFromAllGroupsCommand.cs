using Groups.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Commands.RemoveUserFromAllGroups;

/// <summary>
/// Comando de sistema: elimina un usuario de todos sus grupos cuando su cuenta se borra.
/// A diferencia de RemoveMemberCommand, no requiere verificación de administrador
/// porque es una operación iniciada por un evento de integración del sistema.
/// </summary>
public sealed record RemoveUserFromAllGroupsCommand(UserId UserId) : IRequest<Result>;
