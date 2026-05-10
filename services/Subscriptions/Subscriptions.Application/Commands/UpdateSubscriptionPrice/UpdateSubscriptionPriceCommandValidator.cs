using FluentValidation;

namespace Subscriptions.Application.Commands.UpdateSubscriptionPrice;

public sealed class UpdateSubscriptionPriceCommandValidator
    : AbstractValidator<UpdateSubscriptionPriceCommand>
{
    public UpdateSubscriptionPriceCommandValidator()
    {
        RuleFor(x => x.SubscriptionId.Value)
            .NotEmpty()
            .WithMessage("El identificador de la suscripción es obligatorio.");

        RuleFor(x => x.AdminId.Value)
            .NotEmpty()
            .WithMessage("El identificador del administrador es obligatorio.");

        RuleFor(x => x.NewAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El nuevo importe no puede ser negativo.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La moneda es obligatoria.")
            .Length(3)
            .WithMessage("La moneda debe ser un código ISO de tres letras (por ejemplo, EUR, USD).");
    }
}
