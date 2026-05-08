using FluentValidation;

namespace Groups.Application.Commands.CreateGroup;

// El validador comprueba los datos del comando ANTES de que lleguen al handler.
// FluentValidation lo ejecuta automáticamente si se registra el behavior de validación en MediatR.
public sealed class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        // El nombre no puede estar vacío ni ser solo espacios en blanco.
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre del grupo es obligatorio.")
            // Mínimo 3 caracteres.
            .MinimumLength(3)
            .WithMessage("El nombre del grupo debe tener al menos 3 caracteres.")
            // Máximo 100 caracteres.
            .MaximumLength(100)
            .WithMessage("El nombre del grupo no puede superar los 100 caracteres.");

        // El ID del administrador no puede ser un Guid vacío.
        RuleFor(x => x.AdminId.Value)
            .NotEmpty()
            .WithMessage("El identificador del administrador es obligatorio.");

        // El email del administrador debe tener formato válido.
        RuleFor(x => x.AdminEmail)
            .NotEmpty()
            .WithMessage("El email del administrador es obligatorio.")
            .EmailAddress()
            .WithMessage("El email del administrador no tiene un formato válido.");
    }
}
