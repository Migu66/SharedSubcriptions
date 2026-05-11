using Analytics.Domain.ReadModels;
using Analytics.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Persistence.Configurations;

internal sealed class GroupSavingsConfiguration : IEntityTypeConfiguration<GroupSavingsReadModel>
{
    public void Configure(EntityTypeBuilder<GroupSavingsReadModel> builder)
    {
        builder.ToTable("GroupSavings");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedNever();

        builder.Property(g => g.GroupId)
            .HasConversion<GroupIdConverter>()
            .IsRequired();

        builder.Property(g => g.Year)
            .IsRequired();

        builder.Property(g => g.TotalSpent)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(g => g.EstimatedSavings)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.HasIndex(g => new { g.GroupId, g.Year })
            .IsUnique();
    }
}
