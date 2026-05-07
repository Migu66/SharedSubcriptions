using Groups.Domain.Aggregates;
using Groups.Domain.ValueObjects;
using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Repositories;

public interface IInvitationRepository : IRepository<Invitation, InvitationId>
{
    Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default);
}
