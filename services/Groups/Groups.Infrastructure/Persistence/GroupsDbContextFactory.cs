using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Groups.Infrastructure.Persistence;

// Esta clase solo la usa "dotnet ef migrations" en tiempo de desarrollo.
// Nunca se ejecuta en producción.
internal sealed class GroupsDbContextFactory : IDesignTimeDbContextFactory<GroupsDbContext>
{
    public GroupsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GroupsDbContext>()
            .UseSqlServer("Server=localhost;Database=GroupsDb;Trusted_Connection=True;")
            .Options;

        return new GroupsDbContext(options);
    }
}
