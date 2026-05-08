using Groups.Domain.Aggregates;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using MediatR;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Application.Commands.CreateGroup;

// internal sealed: este handler solo existe dentro de la capa Application,
// nadie de fuera puede instanciarlo directamente. MediatR lo resuelve por inyección de dependencias.
internal sealed class CreateGroupCommandHandler
    : IRequestHandler<CreateGroupCommand, Result<GroupId>>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateGroupCommandHandler(
        IGroupRepository groupRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _groupRepository = groupRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<GroupId>> Handle(
        CreateGroupCommand request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Intentar crear el value object GroupName desde el string del comando.
        // Si el nombre no es válido (vacío, muy corto, muy largo), devuelve un error aquí
        // y no se llega a crear el grupo.
        var groupNameResult = GroupName.Create(request.Name);
        if (groupNameResult.IsFailure)
            return Result.Failure<GroupId>(groupNameResult.Error);

        // Paso 2: Llamar al método de fábrica del agregado Group.
        // Aquí ocurre la lógica de dominio: se crea el grupo, se añade al admin como primer miembro
        // y se emite el domain event GroupCreatedEvent.
        var groupResult = Group.Create(
            groupNameResult.Value,
            request.AdminId,
            request.AdminEmail,
            _dateTimeProvider.UtcNow);

        if (groupResult.IsFailure)
            return Result.Failure<GroupId>(groupResult.Error);

        var group = groupResult.Value;

        // Paso 3: Persistir el grupo en la base de datos usando el repositorio.
        await _groupRepository.AddAsync(group, cancellationToken);

        // Paso 4: Confirmar la transacción. SaveChangesAsync guarda todo en la BD
        // y, gracias al patrón Outbox (que se configurará en Infraestructura),
        // también publicará los integration events.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(group.Id);
    }
}
