using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Aggregates;
using Payments.Infrastructure.Persistence.Converters;

namespace Payments.Infrastructure.Persistence.Configurations;

internal sealed class PaymentRecordConfiguration : IEntityTypeConfiguration<PaymentRecord>
{
    public void Configure(EntityTypeBuilder<PaymentRecord> builder)
    {
        builder.ToTable("PaymentRecords");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasConversion<PaymentRecordIdConverter>()
            .ValueGeneratedNever();

        builder.Property(p => p.SubscriptionId)
            .HasConversion<SubscriptionIdConverter>()
            .IsRequired();

        builder.Property(p => p.GroupId)
            .HasConversion<GroupIdConverter>()
            .IsRequired();

        builder.Property(p => p.AdminId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        builder.Property(p => p.PaidAt)
            .IsRequired();

        builder.OwnsOne(p => p.TotalAmount, moneyBuilder =>
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

        builder.OwnsMany(p => p.MemberQuotas, quotaBuilder =>
        {
            quotaBuilder.ToTable("PaymentRecordMemberQuotas");

            quotaBuilder.WithOwner().HasForeignKey("PaymentRecordId");
            quotaBuilder.Property<int>("Id").ValueGeneratedOnAdd();
            quotaBuilder.HasKey("Id");

            quotaBuilder.Property(q => q.MemberId)
                .HasConversion<UserIdConverter>()
                .HasColumnName("MemberId")
                .IsRequired();

            quotaBuilder.Property(q => q.Amount)
                .HasPrecision(18, 4)
                .IsRequired();

            quotaBuilder.Property(q => q.Currency)
                .HasMaxLength(3)
                .IsRequired();

            quotaBuilder.Property(q => q.IsProrrated)
                .IsRequired();
        });
    }
}
