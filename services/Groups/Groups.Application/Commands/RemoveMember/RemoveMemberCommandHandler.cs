using Groups.Domain.Errors;
using Groups.Domain.Repositories;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Commands.RemoveMember;

// internal sealed: solo visible dentro de la capa Application, MediatR lo resuelve por DI.
internal sealed class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveMemberCommandHandler(
        IGroupRepository groupRepository,
        IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveMemberCommand request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Buscar el grupo por su ID.
        // Si no existe, devolvemos un error y no hacemos nada más.
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
            return Result.Failure(GroupErrors.NotFound);

        // Paso 2: Comprobar que quien hace la petición es el administrador del grupo.
        // Solo el admin puede expulsar miembros.
        if (group.AdminId != request.AdminId)
            return Result.Failure(GroupErrors.NotAdmin);

        // Paso 3: Llamar al método de negocio del agregado.
        // El propio agregado comprueba que el miembro existe y que no se está intentando
        // eliminar al administrador, y emite el domain event MemberRemovedEvent.
        var result = group.RemoveMember(request.MemberToRemoveId);
        if (result.IsFailure)
            return result;

        // Paso 4: Persistir los cambios y confirmar la transacción.
        _groupRepository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
