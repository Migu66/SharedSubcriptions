using Analytics.Domain.ReadModels;
using Analytics.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Persistence.Configurations;

internal sealed class ServiceSpendingConfiguration : IEntityTypeConfiguration<ServiceSpendingReadModel>
{
    public void Configure(EntityTypeBuilder<ServiceSpendingReadModel> builder)
    {
        builder.ToTable("ServiceSpendings");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.GroupId)
            .HasConversion<GroupIdConverter>()
            .IsRequired();

        builder.Property(s => s.ServiceName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.TotalSpent)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(s => s.PaymentCount)
            .IsRequired();

        builder.HasIndex(s => new { s.GroupId, s.ServiceName })
            .IsUnique();
    }
}
