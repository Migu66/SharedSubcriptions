using Groups.Domain.Aggregates;
using Groups.Domain.Enums;
using Groups.Domain.ValueObjects;
using Groups.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groups.Infrastructure.Persistence.Configurations;

internal sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasConversion<InvitationIdConverter>()
            .ValueGeneratedNever();

        builder.Property(i => i.GroupId)
            .HasConversion<GroupIdConverter>()
            .IsRequired();

        builder.Property(i => i.InviteeEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .IsRequired();

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.Ignore(i => i.DomainEvents);
    }
}
