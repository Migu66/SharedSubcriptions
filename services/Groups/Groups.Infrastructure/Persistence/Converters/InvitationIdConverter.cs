using Groups.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Groups.Infrastructure.Persistence.Converters;

internal sealed class InvitationIdConverter()
    : ValueConverter<InvitationId, Guid>(
        invitationId => invitationId.Value,
        value => new InvitationId(value));
