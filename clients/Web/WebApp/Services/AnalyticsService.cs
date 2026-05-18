using System.Net.Http.Json;
using System.Security.Claims;
using SharedUI.Models;

namespace WebApp.Services;

/// <summary>
/// Obtiene del API Gateway los datos de analíticas: ahorro anual y gasto por servicio.
/// </summary>
public sealed class AnalyticsService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AnalyticsService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Devuelve los grupos de los que el usuario actual es administrador.</summary>
    public async Task<IReadOnlyList<AdminGroupItem>> GetAdminGroupsAsync(CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return [];

        var client = _httpClientFactory.CreateClient("ApiGateway");

        var groups = await client.GetFromJsonAsync<List<GroupSummaryResponse>>(
            $"/api/groups/user/{userId}", ct) ?? [];

        // Obtener detalles de cada grupo para saber si el usuario es admin
        var detailTasks = groups.Select(g =>
            client.GetFromJsonAsync<GroupDetailsResponse>($"/api/groups/{g.Id}", ct));

        var details = await Task.WhenAll(detailTasks);

        return details
            .Where(d => d is not null && d.AdminId == userId)
            .Select(d => new AdminGroupItem(d!.Id, d.Name))
            .ToList();
    }

    /// <summary>Devuelve el modelo de ahorro anual para un grupo y año concretos.</summary>
    public async Task<GroupSavingsModel?> GetGroupSavingsAsync(
        Guid groupId, int year, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");

        var response = await client.GetFromJsonAsync<GroupSavingsResponse>(
            $"/api/analytics/groups/{groupId}/savings?year={year}", ct);

        if (response is null) return null;

        return new GroupSavingsModel(
            GroupId: response.GroupId,
            Year: response.Year,
            TotalSpent: response.TotalSpent,
            EstimatedSavings: response.EstimatedSavings,
            Currency: response.Currency);
    }

    /// <summary>Devuelve el desglose de gasto por servicio para un grupo.</summary>
    public async Task<IReadOnlyList<ServiceSpendingItem>> GetServiceSpendingAsync(
        Guid groupId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");

        var items = await client.GetFromJsonAsync<List<ServiceSpendingResponse>>(
            $"/api/analytics/groups/{groupId}/spending", ct) ?? [];

        return items.Select(i => new ServiceSpendingItem(
            i.ServiceName,
            i.TotalSpent,
            i.PaymentCount,
            i.Currency)).ToList();
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    // ── DTOs de respuesta (internos al servicio) ─────────────────────────────

    private sealed record GroupSummaryResponse(Guid Id, string Name, int MemberCount, string UserRole);
    private sealed record GroupDetailsResponse(Guid Id, string Name, Guid AdminId, DateTime CreatedAt,
        List<MemberResponse> Members);
    private sealed record MemberResponse(Guid Id, string Email, string Role, DateTime JoinedAt,
        string PaymentStatus);
    private sealed record GroupSavingsResponse(Guid GroupId, int Year, decimal TotalSpent,
        decimal EstimatedSavings, string Currency);
    private sealed record ServiceSpendingResponse(string ServiceName, decimal TotalSpent,
        int PaymentCount, string Currency);
}

// ── Modelos públicos del servicio ────────────────────────────────────────────

public sealed record AdminGroupItem(Guid Id, string Name);

public sealed record ServiceSpendingItem(
    string ServiceName,
    decimal TotalSpent,
    int PaymentCount,
    string Currency);
