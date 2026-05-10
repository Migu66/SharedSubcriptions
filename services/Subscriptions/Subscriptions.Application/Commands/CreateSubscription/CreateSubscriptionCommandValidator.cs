using FluentValidation;

namespace Subscriptions.Application.Commands.CreateSubscription;

public sealed class CreateSubscriptionCommandValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionCommandValidator()
    {
        RuleFor(x => x.GroupId.Value)
            .NotEmpty()
            .WithMessage("El identificador del grupo es obligatorio.");

        RuleFor(x => x.AdminId.Value)
            .NotEmpty()
            .WithMessage("El identificador del administrador es obligatorio.");

        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .WithMessage("El nombre del servicio es obligatorio.")
            .MaximumLength(100)
            .WithMessage("El nombre del servicio no puede superar los 100 caracteres.");

        RuleFor(x => x.TotalCost)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El coste total no puede ser negativo.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La moneda es obligatoria.")
            .Length(3)
            .WithMessage("La moneda debe ser un código ISO de tres letras (por ejemplo, EUR, USD).");

        RuleFor(x => x.FirstBillingDate)
            .NotEqual(default(DateTime))
            .WithMessage("La fecha del primer cobro no es válida.");
    }
}
