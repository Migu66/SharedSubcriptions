using FluentValidation;

namespace Payments.Application.Commands.ConfirmAdminPayment;

public sealed class ConfirmAdminPaymentCommandValidator : AbstractValidator<ConfirmAdminPaymentCommand>
{
    public ConfirmAdminPaymentCommandValidator()
    {
        RuleFor(x => x.SubscriptionId.Value)
            .NotEmpty()
            .WithMessage("El identificador de la suscripción es obligatorio.");

        RuleFor(x => x.GroupId.Value)
            .NotEmpty()
            .WithMessage("El identificador del grupo es obligatorio.");

        RuleFor(x => x.AdminId.Value)
            .NotEmpty()
            .WithMessage("El identificador del administrador es obligatorio.");

        RuleFor(x => x.MemberIds)
            .NotEmpty()
            .WithMessage("Debe haber al menos un miembro para generar las cuotas.");

        RuleForEach(x => x.MemberIds)
            .NotEmpty()
            .WithMessage("Los identificadores de miembro no pueden estar vacíos.");

        RuleFor(x => x.TotalAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El importe total no puede ser negativo.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La moneda es obligatoria.")
            .Length(3)
            .WithMessage("La moneda debe ser un código ISO de tres letras.");

        RuleFor(x => x.PaidAt)
            .NotEmpty()
            .WithMessage("La fecha de pago es obligatoria.");
    }
}
