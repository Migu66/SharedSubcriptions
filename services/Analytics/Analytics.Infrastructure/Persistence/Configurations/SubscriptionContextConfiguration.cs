using Analytics.Domain.ReadModels;
using Analytics.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Persistence.Configurations;

internal sealed class SubscriptionContextConfiguration : IEntityTypeConfiguration<SubscriptionContextReadModel>
{
    public void Configure(EntityTypeBuilder<SubscriptionContextReadModel> builder)
    {
        builder.ToTable("SubscriptionContexts");

        builder.HasKey(s => s.SubscriptionId);

        builder.Property(s => s.SubscriptionId)
            .ValueGeneratedNever();

        builder.Property(s => s.GroupId)
            .HasConversion<GroupIdConverter>()
            .IsRequired();

        builder.Property(s => s.ServiceName)
            .HasMaxLength(200)
            .IsRequired();
    }
}
