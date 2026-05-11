using Microsoft.Extensions.Configuration;
using Notifications.Application.Abstractions;
using SharedSubscriptions.SharedKernel.Domain;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace Notifications.Infrastructure.Services;

internal sealed class TelegramBotSender : ITelegramSender
{
    private readonly ITelegramBotClient _client;

    public TelegramBotSender(IConfiguration configuration)
    {
        var botToken = configuration["Notifications:Telegram:BotToken"]
            ?? throw new InvalidOperationException("Falta la configuración 'Notifications:Telegram:BotToken'.");

        _client = new TelegramBotClient(botToken);
    }

    public async Task<Result> SendAsync(
        string chatId,
        string message,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _client.SendMessage(
                chatId: chatId,
                text: message,
                cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (ApiRequestException ex)
        {
            return Result.Failure(new Error(
                "Telegram.SendFailed",
                $"El envío de mensaje de Telegram falló: {ex.Message}"));
        }
    }
}
