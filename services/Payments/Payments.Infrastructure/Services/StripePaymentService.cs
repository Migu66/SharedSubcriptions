using Microsoft.Extensions.Configuration;
using Payments.Application.Abstractions;
using Payments.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;
using Stripe;

namespace Payments.Infrastructure.Services;

internal sealed class StripePaymentService : IStripePaymentService
{
    private readonly PaymentIntentService _paymentIntentService;

    public StripePaymentService(IConfiguration configuration)
    {
        var secretKey = configuration["Stripe:SecretKey"]
            ?? throw new InvalidOperationException("Falta la configuración 'Stripe:SecretKey'.");

        StripeConfiguration.ApiKey = secretKey;
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<Result<string>> CreatePaymentIntentAsync(
        Money amount,
        UserId debtorId,
        DebtId debtId,
        CancellationToken cancellationToken = default)
    {
        // Stripe trabaja con importes en la unidad más pequeña de la moneda (céntimos para EUR).
        var amountInCents = (long)(amount.Amount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = amount.Currency.ToLower(),
            Metadata = new Dictionary<string, string>
            {
                { "debtorId", debtorId.Value.ToString() },
                { "debtId", debtId.Value.ToString() }
            }
        };

        var intent = await _paymentIntentService.CreateAsync(
            options,
            cancellationToken: cancellationToken);

        return Result.Success(intent.ClientSecret);
    }
}
