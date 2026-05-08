using Groups.Domain.Aggregates;
using Groups.Domain.Enums;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Groups.Infrastructure.Persistence.Repositories;

internal sealed class InvitationRepository : IInvitationRepository
{
    private readonly GroupsDbContext _context;

    public InvitationRepository(GroupsDbContext context)
    {
        _context = context;
    }

    public async Task<Invitation?> GetByIdAsync(InvitationId id, CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task AddAsync(Invitation aggregate, CancellationToken cancellationToken = default)
    {
        await _context.Invitations.AddAsync(aggregate, cancellationToken);
    }

    public void Update(Invitation aggregate)
    {
        _context.Invitations.Update(aggregate);
    }

    public void Remove(Invitation aggregate)
    {
        _context.Invitations.Remove(aggregate);
    }

    public async Task<IReadOnlyList<Invitation>> GetPendingByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .Where(i => i.InviteeEmail == email && i.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken);
    }
}
