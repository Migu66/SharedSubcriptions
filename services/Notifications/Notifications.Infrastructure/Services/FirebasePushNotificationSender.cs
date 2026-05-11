using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Notifications.Application.Abstractions;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Infrastructure.Services;

internal sealed class FirebasePushNotificationSender : IPushNotificationSender
{
    private readonly FirebaseMessaging _messaging;

    public FirebasePushNotificationSender(IConfiguration configuration)
    {
        var credentialPath = configuration["Notifications:Firebase:CredentialPath"]
            ?? throw new InvalidOperationException("Falta la configuración 'Notifications:Firebase:CredentialPath'.");

        if (FirebaseApp.DefaultInstance is null)
        {
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(credentialPath)
            });
        }

        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task<Result> SendAsync(
        string deviceToken,
        string title,
        string body,
        CancellationToken cancellationToken = default)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        var result = await _messaging.SendAsync(message, cancellationToken);

        if (string.IsNullOrEmpty(result))
            return Result.Failure(new Error(
                "Push.SendFailed",
                "El envío de la notificación push no devolvió un identificador válido."));

        return Result.Success();
    }
}
