using System.Net.Http.Json;
using System.Security.Claims;
using SharedUI.Models;

namespace WebApp.Services;

/// <summary>
/// Gestiona las operaciones CRUD de grupos contra el API Gateway.
/// </summary>
public sealed class GroupService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GroupService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<GroupDetailResult?> GetGroupDetailAsync(Guid groupId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");

        var groupTask = client.GetFromJsonAsync<GroupDetailsResponse>($"/api/groups/{groupId}", ct);
        var subsTask = client.GetFromJsonAsync<List<SubscriptionSummaryResponse>>(
            $"/api/subscriptions/group/{groupId}", ct);

        await Task.WhenAll(groupTask, subsTask);

        var group = groupTask.Result;
        var subs = subsTask.Result ?? [];

        if (group is null) return null;

        var currentUserId = GetCurrentUserId();
        var isAdmin = group.AdminId == currentUserId;

        var members = group.Members.Select(m => new MemberModel(
            m.Id, m.Email, m.Role, m.JoinedAt,
            MapPaymentStatus(m.PaymentStatus)
        )).ToList();

        var groupModel = new GroupModel(group.Id, group.Name, group.AdminId, group.CreatedAt, members);

        var subscriptions = subs.Select(s => new SubscriptionModel(
            s.Id, s.ServiceName, s.TotalCost, s.Currency,
            s.BillingCycle, s.NextBillingDate, s.IsActive, s.IndividualQuota
        )).ToList();

        return new GroupDetailResult(groupModel, subscriptions, isAdmin);
    }

    public async Task<Guid?> CreateGroupAsync(string name, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var adminId = GetCurrentUserId();

        var response = await client.PostAsJsonAsync("/api/groups", new { name, adminId }, ct);

        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<CreateGroupResponse>(cancellationToken: ct);
        return result?.Id;
    }

    public async Task<bool> AddMemberAsync(Guid groupId, string email, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var adminId = GetCurrentUserId();

        var response = await client.PostAsJsonAsync(
            $"/api/groups/{groupId}/members",
            new { adminId, inviteeEmail = email },
            ct);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveMemberAsync(Guid groupId, Guid memberId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var adminId = GetCurrentUserId();

        var response = await client.DeleteAsync(
            $"/api/groups/{groupId}/members/{memberId}?adminId={adminId}", ct);

        return response.IsSuccessStatusCode;
    }

    private Guid GetCurrentUserId()
    {
        var str = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(str, out var id) ? id : Guid.Empty;
    }

    private static PaymentStatus MapPaymentStatus(string status) => status switch
    {
        "Green" => PaymentStatus.Green,
        "Yellow" => PaymentStatus.Yellow,
        "Red" => PaymentStatus.Red,
        _ => PaymentStatus.Yellow
    };

    // ── Respuestas API ───────────────────────────────────────────────────────

    private sealed record GroupDetailsResponse(
        Guid Id, string Name, Guid AdminId, DateTime CreatedAt,
        List<MemberResponse> Members);

    private sealed record MemberResponse(Guid Id, string Email, string Role, DateTime JoinedAt, string PaymentStatus);

    private sealed record SubscriptionSummaryResponse(
        Guid Id, string ServiceName, decimal TotalCost, string Currency,
        string BillingCycle, DateTime NextBillingDate, bool IsActive, decimal IndividualQuota);

    private sealed record CreateGroupResponse(Guid Id);
}

public sealed record GroupDetailResult(
    GroupModel Group,
    IReadOnlyList<SubscriptionModel> Subscriptions,
    bool IsAdmin);
