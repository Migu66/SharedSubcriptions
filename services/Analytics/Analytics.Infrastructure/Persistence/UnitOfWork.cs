using SharedSubscriptions.SharedKernel.Domain;

namespace Analytics.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly AnalyticsDbContext _context;

    public UnitOfWork(AnalyticsDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
