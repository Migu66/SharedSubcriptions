using Analytics.Domain.ReadModels;
using Analytics.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Persistence.Configurations;

internal sealed class DebtHistoryConfiguration : IEntityTypeConfiguration<DebtHistoryReadModel>
{
    public void Configure(EntityTypeBuilder<DebtHistoryReadModel> builder)
    {
        builder.ToTable("DebtHistories");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .ValueGeneratedNever();

        builder.Property(d => d.UserId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        builder.Property(d => d.TotalDebt)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(d => d.TotalSettled)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(d => d.PendingCount)
            .IsRequired();

        builder.HasIndex(d => d.UserId)
            .IsUnique();
    }
}
