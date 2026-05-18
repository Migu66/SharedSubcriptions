using System.Net.Http.Json;
using SharedUI.Models;

namespace MobileApp.Services;

public sealed class MobileDashboardService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MobileAuthService _authService;

    public MobileDashboardService(IHttpClientFactory httpClientFactory, MobileAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    public async Task<IReadOnlyList<GroupDashboardData>> GetDashboardDataAsync(CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var userId = await _authService.GetUserIdAsync();
        if (userId == Guid.Empty) return [];

        var groups = await client.GetFromJsonAsync<List<GroupSummaryResponse>>(
            $"/api/groups/user/{userId}", ct) ?? [];

        var tasks = groups.Select(g => LoadGroupDataAsync(client, g.Id, userId, ct));
        var results = await Task.WhenAll(tasks);

        return results.Where(r => r is not null).Select(r => r!).ToList();
    }

    private async Task<GroupDashboardData?> LoadGroupDataAsync(
        HttpClient client, Guid groupId, Guid currentUserId, CancellationToken ct)
    {
        var groupTask = client.GetFromJsonAsync<GroupDetailsResponse>($"/api/groups/{groupId}", ct);
        var subsTask = client.GetFromJsonAsync<List<SubscriptionSummaryResponse>>(
            $"/api/subscriptions/group/{groupId}", ct);

        await Task.WhenAll(groupTask, subsTask);

        var groupDetails = groupTask.Result;
        var subscriptions = subsTask.Result ?? [];

        if (groupDetails is null) return null;

        var members = groupDetails.Members.Select(m => new MemberModel(
            m.Id, m.Email, m.Role, m.JoinedAt, MapPaymentStatus(m.PaymentStatus))).ToList();

        var groupModel = new GroupModel(
            groupDetails.Id, groupDetails.Name, groupDetails.AdminId,
            groupDetails.CreatedAt, members);

        var subscriptionModels = subscriptions.Select(s => new SubscriptionModel(
            s.Id, s.ServiceName, s.TotalCost, s.Currency,
            s.BillingCycle, s.NextBillingDate, s.IsActive, s.IndividualQuota)).ToList();

        return new GroupDashboardData(groupModel, subscriptionModels,
            IsAdmin: groupDetails.AdminId == currentUserId);
    }

    private static PaymentStatus MapPaymentStatus(string status) => status switch
    {
        "Green"  => PaymentStatus.Green,
        "Yellow" => PaymentStatus.Yellow,
        _        => PaymentStatus.Red
    };

    // ── DTOs de respuesta ────────────────────────────────────────────────────
    private sealed record GroupSummaryResponse(Guid Id, string Name, int MemberCount, string UserRole);
    private sealed record GroupDetailsResponse(Guid Id, string Name, Guid AdminId, DateTime CreatedAt,
        List<MemberResponse> Members);
    private sealed record MemberResponse(Guid Id, string Email, string Role, DateTime JoinedAt,
        string PaymentStatus);
    private sealed record SubscriptionSummaryResponse(Guid Id, string ServiceName, decimal TotalCost,
        string Currency, string BillingCycle, DateTime NextBillingDate, bool IsActive, decimal IndividualQuota);
}

public sealed record GroupDashboardData(
    GroupModel Group,
    IReadOnlyList<SubscriptionModel> Subscriptions,
    bool IsAdmin);
