using System.Net.Http.Json;
using SharedUI.Models;

namespace MobileApp.Services;

public sealed class MobileGroupService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly MobileAuthService _authService;

    public MobileGroupService(IHttpClientFactory httpClientFactory, MobileAuthService authService)
    {
        _httpClientFactory = httpClientFactory;
        _authService = authService;
    }

    public async Task<GroupDetailResult?> GetGroupDetailAsync(Guid groupId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var currentUserId = await _authService.GetUserIdAsync();

        var groupTask = client.GetFromJsonAsync<GroupDetailsResponse>($"/api/groups/{groupId}", ct);
        var subsTask = client.GetFromJsonAsync<List<SubscriptionSummaryResponse>>(
            $"/api/subscriptions/group/{groupId}", ct);

        await Task.WhenAll(groupTask, subsTask);

        var group = groupTask.Result;
        var subs = subsTask.Result ?? [];

        if (group is null) return null;

        var members = group.Members.Select(m => new MemberModel(
            m.Id, m.Email, m.Role, m.JoinedAt, MapPaymentStatus(m.PaymentStatus))).ToList();

        var groupModel = new GroupModel(
            group.Id, group.Name, group.AdminId, group.CreatedAt, members);

        var subscriptions = subs.Select(s => new SubscriptionModel(
            s.Id, s.ServiceName, s.TotalCost, s.Currency,
            s.BillingCycle, s.NextBillingDate, s.IsActive, s.IndividualQuota)).ToList();

        return new GroupDetailResult(groupModel, subscriptions,
            IsAdmin: group.AdminId == currentUserId);
    }

    public async Task<Guid?> CreateGroupAsync(string name, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var adminId = await _authService.GetUserIdAsync();

        var response = await client.PostAsJsonAsync("/api/groups",
            new { Name = name, AdminId = adminId }, ct);

        if (!response.IsSuccessStatusCode) return null;

        var result = await response.Content.ReadFromJsonAsync<CreateGroupResponse>(
            cancellationToken: ct);
        return result?.Id;
    }

    public async Task<bool> AddMemberAsync(Guid groupId, string email, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var adminId = await _authService.GetUserIdAsync();

        var response = await client.PostAsJsonAsync($"/api/groups/{groupId}/members",
            new { AdminId = adminId, InviteeEmail = email }, ct);

        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveMemberAsync(Guid groupId, Guid memberId, CancellationToken ct = default)
    {
        var client = _httpClientFactory.CreateClient("ApiGateway");
        var adminId = await _authService.GetUserIdAsync();

        var response = await client.SendAsync(
            new HttpRequestMessage(HttpMethod.Delete,
                $"/api/groups/{groupId}/members/{memberId}")
            {
                Content = JsonContent.Create(new { AdminId = adminId })
            }, ct);

        return response.IsSuccessStatusCode;
    }

    private static PaymentStatus MapPaymentStatus(string status) => status switch
    {
        "Green"  => PaymentStatus.Green,
        "Yellow" => PaymentStatus.Yellow,
        _        => PaymentStatus.Red
    };

    // ── DTOs de respuesta ────────────────────────────────────────────────────
    private sealed record GroupDetailsResponse(Guid Id, string Name, Guid AdminId, DateTime CreatedAt,
        List<MemberResponse> Members);
    private sealed record MemberResponse(Guid Id, string Email, string Role, DateTime JoinedAt,
        string PaymentStatus);
    private sealed record SubscriptionSummaryResponse(Guid Id, string ServiceName, decimal TotalCost,
        string Currency, string BillingCycle, DateTime NextBillingDate, bool IsActive, decimal IndividualQuota);
    private sealed record CreateGroupResponse(Guid Id);
}

public sealed record GroupDetailResult(
    GroupModel Group,
    IReadOnlyList<SubscriptionModel> Subscriptions,
    bool IsAdmin);
