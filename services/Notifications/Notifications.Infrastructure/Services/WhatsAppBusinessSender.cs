using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Notifications.Application.Abstractions;
using SharedSubscriptions.SharedKernel.Domain;

namespace Notifications.Infrastructure.Services;

internal sealed class WhatsAppBusinessSender : IWhatsAppSender
{
    private readonly HttpClient _httpClient;
    private readonly string _phoneNumberId;

    public WhatsAppBusinessSender(IConfiguration configuration, HttpClient httpClient)
    {
        var accessToken = configuration["Notifications:WhatsApp:AccessToken"]
            ?? throw new InvalidOperationException("Falta la configuración 'Notifications:WhatsApp:AccessToken'.");

        _phoneNumberId = configuration["Notifications:WhatsApp:PhoneNumberId"]
            ?? throw new InvalidOperationException("Falta la configuración 'Notifications:WhatsApp:PhoneNumberId'.");

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://graph.facebook.com/v19.0/");
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<Result> SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            messaging_product = "whatsapp",
            to = phoneNumber,
            type = "text",
            text = new { body = message }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(
            $"{_phoneNumberId}/messages",
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return Result.Failure(new Error(
                "WhatsApp.SendFailed",
                $"El envío de WhatsApp falló con el código {(int)response.StatusCode}."));

        return Result.Success();
    }
}
