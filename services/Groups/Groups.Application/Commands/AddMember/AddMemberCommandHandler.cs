using Groups.Domain.Errors;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Commands.AddMember;

// internal sealed: solo visible dentro de la capa Application, MediatR lo resuelve por DI.
internal sealed class AddMemberCommandHandler : IRequestHandler<AddMemberCommand, Result>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddMemberCommandHandler(
        IGroupRepository groupRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(
        AddMemberCommand request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Buscar el grupo por su ID en la base de datos.
        // Si no existe, devolvemos un error y no hacemos nada más.
        var group = await _groupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
            return Result.Failure(GroupErrors.NotFound);

        // Paso 2: Comprobar que quien hace la petición es el administrador del grupo.
        // Si AdminId no coincide con el AdminId del grupo, rechazamos la operación.
        if (group.AdminId != request.AdminId)
            return Result.Failure(GroupErrors.NotAdmin);

        // Paso 3: Llamar al método de negocio del agregado.
        // El propio agregado comprueba si el miembro ya existe y emite el domain event MemberAddedEvent.
        var result = group.AddMember(
            request.NewMemberId,
            request.InviteeEmail,
            _dateTimeProvider.UtcNow);

        if (result.IsFailure)
            return result;

        // Paso 4: Persistir los cambios del agregado y confirmar la transacción.
        _groupRepository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
