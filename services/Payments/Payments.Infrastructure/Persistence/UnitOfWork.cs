using SharedSubscriptions.SharedKernel.Domain;

namespace Payments.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly PaymentsDbContext _context;

    public UnitOfWork(PaymentsDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
