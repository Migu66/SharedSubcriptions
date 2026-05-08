using Groups.Domain.Aggregates;
using Groups.Domain.Enums;
using Groups.Domain.ValueObjects;
using Groups.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Groups.Infrastructure.Persistence.Configurations;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("Groups");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .HasConversion<GroupIdConverter>()
            .ValueGeneratedNever();

        builder.Property(g => g.AdminId)
            .HasConversion<UserIdConverter>()
            .IsRequired();

        // GroupName se mapea como owned type: en la BD es solo una columna "Name"
        builder.OwnsOne(g => g.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.Value)
                .HasColumnName("Name")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.Property(g => g.CreatedAt)
            .IsRequired();

        // La colección de Member se mapea como owned entity collection
        // Cada Member vive en la tabla Groups_Members
        builder.OwnsMany(g => g.Members, memberBuilder =>
        {
            memberBuilder.ToTable("Groups_Members");

            memberBuilder.WithOwner().HasForeignKey("GroupId");

            memberBuilder.HasKey("Id", "GroupId");

            memberBuilder.Property(m => m.Id)
                .HasConversion<UserIdConverter>()
                .HasColumnName("UserId")
                .ValueGeneratedNever();

            memberBuilder.Property(m => m.Email)
                .HasMaxLength(256)
                .IsRequired();

            memberBuilder.Property(m => m.Role)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            memberBuilder.Property(m => m.JoinedAt)
                .IsRequired();
        });

        // Ignoramos los DomainEvents: son solo para uso en memoria, no se persisten aquí
        builder.Ignore(g => g.DomainEvents);
    }
}
