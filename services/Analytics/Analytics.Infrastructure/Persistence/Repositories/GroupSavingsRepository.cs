using Analytics.Domain.ReadModels;
using Analytics.Domain.Repositories;
using Analytics.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence.Repositories;

internal sealed class GroupSavingsRepository : IGroupSavingsRepository
{
    private readonly AnalyticsDbContext _context;

    public GroupSavingsRepository(AnalyticsDbContext context)
    {
        _context = context;
    }

    public async Task<GroupSavingsReadModel?> GetByGroupIdAndYearAsync(
        GroupId groupId,
        int year,
        CancellationToken cancellationToken = default)
    {
        return await _context.GroupSavings
            .FirstOrDefaultAsync(g => g.GroupId == groupId && g.Year == year, cancellationToken);
    }

    public async Task AddAsync(
        GroupSavingsReadModel readModel,
        CancellationToken cancellationToken = default)
    {
        await _context.GroupSavings.AddAsync(readModel, cancellationToken);
    }

    public Task UpdateAsync(
        GroupSavingsReadModel readModel,
        CancellationToken cancellationToken = default)
    {
        _context.GroupSavings.Update(readModel);
        return Task.CompletedTask;
    }
}
