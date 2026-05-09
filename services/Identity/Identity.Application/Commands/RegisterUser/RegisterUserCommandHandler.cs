using Identity.Domain.Aggregates;
using Identity.Domain.Errors;
using Identity.Domain.Repositories;
using Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Identity;
using SharedSubscriptions.SharedKernel.Domain;

namespace Identity.Application.Commands.RegisterUser;

internal sealed class RegisterUserCommandHandler
    : IRequestHandler<RegisterUserCommand, Result<UserId>>
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterUserCommandHandler(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<UserId>> Handle(
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // Paso 1: Verificar que el email no esté ya en uso.
        bool emailExists = await _userRepository.ExistsWithEmailAsync(request.Email, cancellationToken);
        if (emailExists)
            return Result.Failure<UserId>(UserErrors.EmailAlreadyExists);

        // Paso 2: Crear el agregado ApplicationUser con el método de fábrica.
        // Aquí se emite el domain event UserRegisteredEvent.
        var userResult = ApplicationUser.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            _dateTimeProvider.UtcNow);

        if (userResult.IsFailure)
            return Result.Failure<UserId>(userResult.Error);

        var user = userResult.Value;

        // Paso 3: Persistir mediante UserManager para que la contraseña quede hasheada.
        // UserManager gestiona internamente el SaveChanges a través de su propio store.
        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            var firstError = identityResult.Errors.First();
            return Result.Failure<UserId>(new Error($"Identity.{firstError.Code}", firstError.Description));
        }

        var userId = UserId.From(user.Id);
        if (userId.IsFailure)
            return Result.Failure<UserId>(userId.Error);

        return Result.Success(userId.Value);
    }
}
