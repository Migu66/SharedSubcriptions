using FluentValidation;

namespace Groups.Application.Commands.RemoveMember;

// Comprueba los datos del comando ANTES de que lleguen al handler.
public sealed class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberCommandValidator()
    {
        // El ID del grupo no puede ser un Guid vacío.
        RuleFor(x => x.GroupId.Value)
            .NotEmpty()
            .WithMessage("El identificador del grupo es obligatorio.");

        // El ID del administrador no puede ser un Guid vacío.
        RuleFor(x => x.AdminId.Value)
            .NotEmpty()
            .WithMessage("El identificador del administrador es obligatorio.");

        // El ID del miembro a eliminar no puede ser un Guid vacío.
        RuleFor(x => x.MemberToRemoveId.Value)
            .NotEmpty()
            .WithMessage("El identificador del miembro a eliminar es obligatorio.");
    }
}
