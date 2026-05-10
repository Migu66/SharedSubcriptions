using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Aggregates;
using Payments.Domain.Enums;
using Payments.Infrastructure.Persistence.Converters;

namespace Payments.Infrastructure.Persistence.Configurations;

internal sealed class DebtConfiguration : IEntityTypeConfiguration<Debt>
{
    public void Configure(EntityTypeBuilder<Debt> builder)
    {
        builder.ToTable("Debts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasConversion<DebtIdConverter>()
            .ValueGeneratedNever();

        builder.Property(d => d.PaymentRecordId)
            .HasConversion<PaymentRecordIdConverter>()
            .IsRequired();

        builder.Property(d => d.DebtorId)
            .HasConversion<UserIdConverter>()
            .HasColumnName("DebtorId")
            .IsRequired();

        builder.Property(d => d.CreditorId)
            .HasConversion<UserIdConverter>()
            .HasColumnName("CreditorId")
            .IsRequired();

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .IsRequired();

        builder.Property(d => d.SettledAt);

        builder.OwnsOne(d => d.Amount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 4)
                .IsRequired();

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });
    }
}
