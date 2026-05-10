using SharedSubscriptions.SharedKernel.Domain;

namespace Subscriptions.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly SubscriptionsDbContext _context;

    public UnitOfWork(SubscriptionsDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
