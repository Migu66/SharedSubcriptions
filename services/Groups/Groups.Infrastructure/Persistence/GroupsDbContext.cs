using Groups.Domain.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Groups.Infrastructure.Persistence;

public sealed class GroupsDbContext : DbContext
{
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<Invitation> Invitations => Set<Invitation>();

    public GroupsDbContext(DbContextOptions<GroupsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica automáticamente todas las clases IEntityTypeConfiguration
        // que estén en este mismo ensamblado (Infrastructure)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GroupsDbContext).Assembly);
    }
}
