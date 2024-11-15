namespace EmailParserBot.Handlers;

using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramUpdater.UpdateContainer;
using TelegramUpdater.UpdateHandlers.Scoped.ReadyToUse;

public class ScopedMessageHandler : MessageHandler
{
    private readonly ILogger<ScopedMessageHandler> _logger;

    public ScopedMessageHandler(ILogger<ScopedMessageHandler> logger)
    {
        _logger = logger;
    }

    protected override async Task HandleAsync(IContainer<Message> container)
    { 
        if (container.Update.Type != MessageType.Text) return;

        Message message = container.Update;
        _logger.LogInformation($"Received message: {message.Text} from user with username: {message.From!.Username} with id: {message.From.Id}");

        await ResponseAsync($"Вы успешно зарегестрировались, ваш telegram id: {message.Chat.Id}.");
    }
}