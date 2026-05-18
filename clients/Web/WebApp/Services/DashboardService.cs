using System.Net.Http.Json;
using System.Security.Claims;
using SharedUI;
using SharedUI.Models;

namespace WebApp.Services;

/// <summary>
/// Obtiene del API Gateway los datos necesarios para el dashboard
/// y los transforma en los modelos que usan los componentes de SharedUI.
/// </summary>
public sealed class DashboardService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DashboardService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IReadOnlyList<GroupDashboardData>> GetDashboardDataAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var userId = GetCurrentUserId();

        if (userId == Guid.Empty)
            return [];

        // 1. Obtener todos los grupos del usuario
        var groups = await client.GetFromJsonAsync<List<GroupSummaryResponse>>(
            $"/api/groups/user/{userId}", ct) ?? [];

        var result = new List<GroupDashboardData>();

        // 2. Para cada grupo, obtener detalles completos y suscripciones en paralelo
        var tasks = groups.Select(g => LoadGroupDataAsync(client, g.Id, ct));
        var groupDataItems = await Task.WhenAll(tasks);

        result.AddRange(groupDataItems.Where(x => x is not null)!);
        return result;
    }

    private async Task<GroupDashboardData?> LoadGroupDataAsync(HttpClient client, Guid groupId, CancellationToken ct)
    {
        var currentUserId = GetCurrentUserId();

        var groupTask = client.GetFromJsonAsync<GroupDetailsResponse>($"/api/groups/{groupId}", ct);
        var subsTask = client.GetFromJsonAsync<List<SubscriptionSummaryResponse>>(
            $"/api/subscriptions/group/{groupId}", ct);

        await Task.WhenAll(groupTask, subsTask);

        var groupDetails = groupTask.Result;
        var subscriptions = subsTask.Result ?? [];

        if (groupDetails is null) return null;

        var members = groupDetails.Members.Select(m => new MemberModel(
            m.Id,
            m.Email,
            m.Role,
            m.JoinedAt,
            MapPaymentStatus(m.PaymentStatus)
        )).ToList();

        var groupModel = new GroupModel(
            groupDetails.Id,
            groupDetails.Name,
            groupDetails.AdminId,
            groupDetails.CreatedAt,
            members);

        var subscriptionModels = subscriptions.Select(s => new SubscriptionModel(
            s.Id,
            s.ServiceName,
            s.TotalCost,
            s.Currency,
            s.BillingCycle,
            s.NextBillingDate,
            s.IsActive,
            s.IndividualQuota
        )).ToList();

        return new GroupDashboardData(
            groupModel,
            subscriptionModels,
            IsAdmin: groupDetails.AdminId == currentUserId);
    }

    private Guid GetCurrentUserId()
    {
        var userIdStr = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(userIdStr, out var id) ? id : Guid.Empty;
    }

    private static PaymentStatus MapPaymentStatus(string status) => status switch
    {
        "Green" => PaymentStatus.Green,
        "Yellow" => PaymentStatus.Yellow,
        "Red" => PaymentStatus.Red,
        _ => PaymentStatus.Yellow
    };

    // ── Respuestas de la API ─────────────────────────────────────────────────

    private sealed record GroupSummaryResponse(Guid Id, string Name, int MemberCount, string UserRole);

    private sealed record GroupDetailsResponse(
        Guid Id,
        string Name,
        Guid AdminId,
        DateTime CreatedAt,
        List<MemberResponse> Members);

    private sealed record MemberResponse(Guid Id, string Email, string Role, DateTime JoinedAt, string PaymentStatus);

    private sealed record SubscriptionSummaryResponse(
        Guid Id,
        string ServiceName,
        decimal TotalCost,
        string Currency,
        string BillingCycle,
        DateTime NextBillingDate,
        bool IsActive,
        decimal IndividualQuota);
}

/// <summary>
/// Agrupa el modelo del grupo con sus suscripciones para el dashboard.
/// </summary>
public sealed record GroupDashboardData(
    GroupModel Group,
    IReadOnlyList<SubscriptionModel> Subscriptions,
    bool IsAdmin);
