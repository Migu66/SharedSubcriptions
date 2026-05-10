using FluentValidation;

namespace Payments.Application.Commands.SettleDebt;

public sealed class SettleDebtCommandValidator : AbstractValidator<SettleDebtCommand>
{
    public SettleDebtCommandValidator()
    {
        RuleFor(x => x.DebtId.Value)
            .NotEmpty()
            .WithMessage("El identificador de la deuda es obligatorio.");

        RuleFor(x => x.DebtorId.Value)
            .NotEmpty()
            .WithMessage("El identificador del deudor es obligatorio.");
    }
}
