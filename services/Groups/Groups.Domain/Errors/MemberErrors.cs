using SharedSubscriptions.SharedKernel.Domain;

namespace Groups.Domain.Errors;

public static class MemberErrors
{
    public static readonly Error EmailEmpty = new(
        "Member.EmailEmpty",
        "El email del miembro no puede estar vacío.");
}
