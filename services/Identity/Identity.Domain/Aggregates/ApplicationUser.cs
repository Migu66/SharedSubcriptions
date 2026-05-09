using Identity.Domain.Errors;
using Identity.Domain.Events;
using Identity.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Domain.Aggregates;

/// <summary>
/// Agrega el soporte de domain events a IdentityUser mediante implementación
/// explícita de IHasDomainEvents, ya que C# no permite herencia múltiple.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateTime CreatedAt { get; private init; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private ApplicationUser() { }

    public static Result<ApplicationUser> Create(
        string email,
        string firstName,
        string lastName,
        DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<ApplicationUser>(UserErrors.InvalidEmail);

        if (!email.Contains('@'))
            return Result.Failure<ApplicationUser>(UserErrors.InvalidEmail);

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<ApplicationUser>(UserErrors.NotFound);

        var userId = UserId.New();

        var user = new ApplicationUser
        {
            Id = userId.Value,
            UserName = email,
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant(),
            FirstName = firstName,
            LastName = lastName,
            CreatedAt = createdAt
        };

        user._domainEvents.Add(new UserRegisteredEvent(
            EventId: Guid.NewGuid(),
            OccurredOn: createdAt,
            UserId: userId,
            Email: email,
            FirstName: firstName,
            LastName: lastName));

        return Result.Success(user);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
