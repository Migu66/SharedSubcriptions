using Analytics.Domain.ReadModels;
using Analytics.Domain.Repositories;
using Analytics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence.Repositories;

internal sealed class ServiceSpendingRepository : IServiceSpendingRepository
{
    private readonly AnalyticsDbContext _context;

    public ServiceSpendingRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceSpendingReadModel?> GetByGroupIdAndServiceNameAsync(
        GroupId groupId,
        string serviceName,
        CancellationToken cancellationToken = default)
    {
        return await _context.ServiceSpendings
            .FirstOrDefaultAsync(s => s.GroupId == groupId && s.ServiceName == serviceName, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceSpendingReadModel>> GetByGroupIdAsync(
        GroupId groupId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ServiceSpendings
            .Where(s => s.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        ServiceSpendingReadModel readModel,
        CancellationToken cancellationToken = default)
    {
        await _context.ServiceSpendings.AddAsync(readModel, cancellationToken);
    }

    public Task UpdateAsync(
        ServiceSpendingReadModel readModel,
        CancellationToken cancellationToken = default)
    {
        _context.ServiceSpendings.Update(readModel);
        return Task.CompletedTask;
    }
}
