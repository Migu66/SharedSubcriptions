using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payments.Infrastructure.Persistence;

internal sealed class PaymentsDbContextFactory
    : IDesignTimeDbContextFactory<PaymentsDbContext>
{
    public PaymentsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<PaymentsDbContext>()
            .UseSqlServer("Server=localhost;Database=SharedSubscriptions_Payments;Trusted_Connection=True;TrustServerCertificate=True;")
            .Options;

        return new PaymentsDbContext(options);
    }
}
