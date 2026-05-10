using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payments.Application.Commands.SettleDebt;
using Payments.Domain.ValueObjects;
using Stripe;

namespace Payments.Api.Endpoints;

public static class StripeWebhookEndpoints
{
    public static IEndpointRouteBuilder MapStripeWebhookEndpoints(this IEndpointRouteBuilder app)
    {
        // Este endpoint es público (sin autenticación JWT) porque lo llama Stripe directamente.
        app.MapPost("/api/payments/webhooks/stripe", HandleStripeWebhookAsync)
            .WithName("StripeWebhook")
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> HandleStripeWebhookAsync(
        HttpRequest request,
        IMediator mediator,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var webhookSecret = configuration["Stripe:WebhookSecret"]
            ?? throw new InvalidOperationException("Falta la configuración 'Stripe:WebhookSecret'.");

        string payload;
        using (var reader = new StreamReader(request.Body))
        {
            payload = await reader.ReadToEndAsync(cancellationToken);
        }

        if (!request.Headers.TryGetValue("Stripe-Signature", out var stripeSignature))
        {
            return Results.BadRequest("Falta la cabecera Stripe-Signature.");
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                stripeSignature,
                webhookSecret);
        }
        catch (StripeException)
        {
            return Results.BadRequest("Firma del webhook inválida.");
        }

        if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
        {
            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent)
                return Results.Ok();

            if (!paymentIntent.Metadata.TryGetValue("debtId", out var debtIdRaw) ||
                !Guid.TryParse(debtIdRaw, out var debtIdGuid))
            {
                return Results.BadRequest("El PaymentIntent no contiene un debtId válido.");
            }

            if (!paymentIntent.Metadata.TryGetValue("debtorId", out var debtorIdRaw) ||
                !Guid.TryParse(debtorIdRaw, out var debtorIdGuid))
            {
                return Results.BadRequest("El PaymentIntent no contiene un debtorId válido.");
            }

            var debtIdResult = DebtId.From(debtIdGuid);
            var debtorIdResult = UserId.From(debtorIdGuid);

            if (debtIdResult.IsFailure || debtorIdResult.IsFailure)
                return Results.BadRequest("IDs inválidos en los metadatos del PaymentIntent.");

            var command = new SettleDebtCommand(debtIdResult.Value, debtorIdResult.Value);
            var result = await mediator.Send(command, cancellationToken);

            if (result.IsFailure)
                return Results.UnprocessableEntity(result.Error.Description);
        }

        return Results.Ok();
    }
}
