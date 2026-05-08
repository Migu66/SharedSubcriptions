using FluentValidation;

namespace Groups.Application.Commands.AddMember;

// Comprueba los datos del comando ANTES de que lleguen al handler.
public sealed class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
{
    public AddMemberCommandValidator()
    {
        // El ID del grupo no puede ser un Guid vacío.
        RuleFor(x => x.GroupId.Value)
            .NotEmpty()
            .WithMessage("El identificador del grupo es obligatorio.");

        // El ID del administrador no puede ser un Guid vacío.
        RuleFor(x => x.AdminId.Value)
            .NotEmpty()
            .WithMessage("El identificador del administrador es obligatorio.");

        // El ID del nuevo miembro no puede ser un Guid vacío.
        RuleFor(x => x.NewMemberId.Value)
            .NotEmpty()
            .WithMessage("El identificador del nuevo miembro es obligatorio.");

        // El email debe tener formato válido y no estar vacío.
        RuleFor(x => x.InviteeEmail)
            .NotEmpty()
            .WithMessage("El email del nuevo miembro es obligatorio.")
            .EmailAddress()
            .WithMessage("El email del nuevo miembro no tiene un formato válido.");
    }
}
