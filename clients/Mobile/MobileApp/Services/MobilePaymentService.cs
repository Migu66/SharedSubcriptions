using System.Net.Http.Json;

namespace MobileApp.Services;

public sealed class MobilePaymentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MobileAuthService _authService;

    public MobilePaymentService(IHttpClientFactory httpClientFactory, MobileAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    public async Task<IReadOnlyList<DebtDto>> GetPendingDebtsAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var userId = await _authService.GetUserIdAsync();
        if (userId == Guid.Empty) return [];

        return await client.GetFromJsonAsync<List<DebtDto>>(
            $"/api/payments/debts/pending/{userId}", ct) ?? [];
    }

    public async Task<string?> CreateStripeCheckoutAsync(Guid debtId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var response = await client.PostAsJsonAsync(
            $"/api/payments/debts/{debtId}/stripe-checkout",
            new { debtId }, ct);

        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<StripeCheckoutResponse>(
            cancellationToken: ct);
        return result?.CheckoutUrl;
    }

    public async Task<bool> SettleDebtManuallyAsync(Guid debtId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var userId = await _authService.GetUserIdAsync();
        var response = await client.PostAsJsonAsync(
            $"/api/payments/debts/{debtId}/settle-manual",
            new { CreditorId = userId }, ct);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ConfirmAdminPaymentAsync(
        Guid subscriptionId, decimal amount, string currency, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var adminId = await _authService.GetUserIdAsync();
        var response = await client.PostAsJsonAsync("/api/payments/confirm",
            new { SubscriptionId = subscriptionId, AdminId = adminId,
                  TotalAmount = amount, Currency = currency, PaidAt = DateTime.UtcNow }, ct);

        return response.IsSuccessStatusCode;
    }

    // ── DTOs ────────────────────────────────────────────────────────────────
    private sealed record StripeCheckoutResponse(string CheckoutUrl);
}

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
