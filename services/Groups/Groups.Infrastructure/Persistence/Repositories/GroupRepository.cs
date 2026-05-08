using Groups.Domain.Aggregates;
using Groups.Domain.Repositories;
using Groups.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Groups.Infrastructure.Persistence.Repositories;

internal sealed class GroupRepository : IGroupRepository
{
    private readonly GroupsDbContext _context;

    public GroupRepository(GroupsDbContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }

    public async Task AddAsync(Group aggregate, CancellationToken cancellationToken = default)
    {
        await _context.Groups.AddAsync(aggregate, cancellationToken);
    }

    public void Update(Group aggregate)
    {
        _context.Groups.Update(aggregate);
    }

    public void Remove(Group aggregate)
    {
        _context.Groups.Remove(aggregate);
    }

    public async Task<IReadOnlyList<Group>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Groups
            .Include(g => g.Members)
            .Where(g => g.Members.Any(m => m.Id == userId))
            .ToListAsync(cancellationToken);
    }
}
