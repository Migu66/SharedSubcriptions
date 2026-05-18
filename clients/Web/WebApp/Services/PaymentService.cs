using System.Net.Http.Json;
using System.Security.Claims;

namespace WebApp.Services;

/// <summary>
/// Gestiona las operaciones de pagos y deudas contra el API Gateway.
/// </summary>
public sealed class PaymentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PaymentService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Historial de pagos de una suscripción.</summary>
    public async Task<IReadOnlyList<PaymentRecordDto>> GetPaymentHistoryAsync(
        Guid subscriptionId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        return await client.GetFromJsonAsync<List<PaymentRecordDto>>(
            $"/api/payments/history/{subscriptionId}", ct) ?? [];
    }

    /// <summary>Deudas pendientes del usuario autenticado.</summary>
    public async Task<IReadOnlyList<DebtDto>> GetPendingDebtsAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return [];

        return await client.GetFromJsonAsync<List<DebtDto>>(
            $"/api/payments/debts/pending/{userId}", ct) ?? [];
    }

    /// <summary>
    /// Crea un PaymentIntent en Stripe y devuelve la URL de checkout.
    /// </summary>
    public async Task<string?> CreateStripeCheckoutAsync(Guid debtId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var response = await client.PostAsJsonAsync(
            $"/api/payments/debts/{debtId}/stripe-checkout",
            new { debtId },
            ct);

        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<StripeCheckoutResponse>(cancellationToken: ct);
        return result?.CheckoutUrl;
    }

    /// <summary>El acreedor (admin) marca la deuda como saldada manualmente.</summary>
    public async Task<bool> SettleDebtManuallyAsync(Guid debtId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var creditorId = GetCurrentUserId();

        var response = await client.PostAsJsonAsync(
            $"/api/payments/debts/{debtId}/settle-manual",
            new { creditorId },
            ct);

        return response.IsSuccessStatusCode;
    }

    /// <summary>El administrador confirma que pagó al proveedor.</summary>
    public async Task<bool> ConfirmAdminPaymentAsync(
        Guid subscriptionId, decimal totalAmount, string currency, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var adminId = GetCurrentUserId();

        var response = await client.PostAsJsonAsync("/api/payments/confirm", new
        {
            subscriptionId,
            adminId,
            totalAmount,
            currency,
            paidAt = DateTime.UtcNow
        }, ct);

        return response.IsSuccessStatusCode;
    }

    private Guid GetCurrentUserId()
    {
        var str = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(str, out var id) ? id : Guid.Empty;
    }

    // ── DTOs de respuesta ────────────────────────────────────────────────────

    private sealed record StripeCheckoutResponse(string CheckoutUrl);
}

public sealed record PaymentRecordDto(
    Guid Id,
    Guid SubscriptionId,
    string ServiceName,
    decimal TotalAmount,
    string Currency,
    DateTime PaidAt,
    IReadOnlyList<MemberQuotaDto> MemberQuotas);

public sealed record MemberQuotaDto(
    Guid MemberId,
    string MemberEmail,
    decimal Amount,
    string Currency,
    bool IsProrrated);

public sealed record DebtDto(
    Guid Id,
    Guid SubscriptionId,
    string ServiceName,
    decimal Amount,
    string Currency,
    Guid DebtorId,
    Guid CreditorId,
    string CreditorEmail,
    string Status,
    DateTime CreatedAt,
    DateTime? SettledAt);
