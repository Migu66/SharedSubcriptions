using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly GroupsDbContext _context;

    public UnitOfWork(GroupsDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
