namespace Identity.Domain.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public string Token { get; private init; } = null!;
    public DateTime ExpiresAt { get; private init; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private init; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid userId, string token, DateTime expiresAt, DateTime createdAt) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            CreatedAt = createdAt
        };

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);

    public void Revoke() => IsRevoked = true;
}
