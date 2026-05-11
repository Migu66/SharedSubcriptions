using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Analytics.Infrastructure.Persistence;

internal sealed class AnalyticsDbContextFactory
    : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=SharedSubscriptions_Analytics;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;")
            .Options;

        return new AnalyticsDbContext(options);
    }
}
