using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscriptions.Domain.Aggregates;
using Subscriptions.Domain.Enums;
using Subscriptions.Infrastructure.Persistence.Converters;

namespace Subscriptions.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion<SubscriptionIdConverter>()
            .ValueGeneratedNever();

        builder.Property(s => s.GroupId)
            .HasConversion<GroupIdConverter>()
            .IsRequired();

        builder.Property(s => s.ServiceName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.OwnsOne(s => s.TotalCost, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("TotalCostAmount")
                .HasPrecision(18, 4)
                .IsRequired();

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("TotalCostCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.OwnsOne(s => s.BillingSchedule, scheduleBuilder =>
        {
            scheduleBuilder.Property(b => b.Cycle)
                .HasColumnName("BillingCycle")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            scheduleBuilder.Property(b => b.NextBillingDate)
                .HasColumnName("NextBillingDate")
                .IsRequired();
        });
    }
}
