using Groups.Domain.Repositories;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Commands.RemoveUserFromAllGroups;

internal sealed class RemoveUserFromAllGroupsCommandHandler
    : IRequestHandler<RemoveUserFromAllGroupsCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveUserFromAllGroupsCommandHandler(
        IGroupRepository groupRepository,
        IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveUserFromAllGroupsCommand request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Obtener todos los grupos en los que el usuario es miembro.
        var groups = await _groupRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (groups.Count == 0)
            return Result.Success();

        // Paso 2: En cada grupo, intentar eliminar al usuario.
        // Si el usuario es el administrador del grupo, se omite porque el dominio
        // prohíbe eliminar al admin (GroupErrors.AdminCannotBeRemoved).
        // El tratamiento de grupos sin admin queda fuera del alcance de esta fase.
        foreach (var group in groups)
        {
            var result = group.RemoveMember(request.UserId);

            if (result.IsFailure)
                continue; // El usuario era admin de este grupo; se omite silenciosamente.

            _groupRepository.Update(group);
        }

        // Paso 3: Persistir todos los cambios en una sola transacción.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
