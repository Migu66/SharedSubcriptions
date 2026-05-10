using FluentValidation;

namespace Payments.Application.Commands.SettleDebtManually;

public sealed class SettleDebtManuallyCommandValidator : AbstractValidator<SettleDebtManuallyCommand>
{
    public SettleDebtManuallyCommandValidator()
    {
        RuleFor(x => x.DebtId.Value)
            .NotEmpty()
            .WithMessage("El identificador de la deuda es obligatorio.");

        RuleFor(x => x.CreditorId.Value)
            .NotEmpty()
            .WithMessage("El identificador del acreedor es obligatorio.");
    }
}
