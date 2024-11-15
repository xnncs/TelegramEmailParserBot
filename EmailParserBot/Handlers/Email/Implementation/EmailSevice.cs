namespace EmailParserBot.Handlers.Email.Implementation;

using System.Text;
using Abstract;
using Contracts;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly ITelegramBotClient _botClient;
    
    private readonly AdminsListContract _adminsContract;

    public EmailService(ILogger<EmailService> logger, ITelegramBotClient botClient, IOptions<AdminsListContract> admins)
    {
        _logger = logger;
        _botClient = botClient;
        _adminsContract = admins.Value;
    }

    public async Task OnEmailAsync(IMessageSummary messageSummary, ImapClient imapClient)
    {
        try
        {
            MimeMessage message = await imapClient.Inbox.GetMessageAsync(messageSummary.UniqueId);
            _logger.LogInformation($"Received a message from: {message.From} with subject: {message.Subject} and body:\n{message.TextBody}");

            await OnMessageLogicAsync(message);
            await imapClient.Inbox.AddFlagsAsync(messageSummary.UniqueId, MessageFlags.Seen, true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Handled exception, while handling email:" + ex.Message);
        }
    }

    private async Task OnMessageLogicAsync(MimeMessage message)
    {
        string outputMessageText = GetOutputTextFromMessage(message);

        foreach (long admin in _adminsContract.Admins)
        {
            await _botClient.SendTextMessageAsync(admin, outputMessageText);
        }
        
    }

    private string GetOutputTextFromMessage(MimeMessage message)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"Новое сообщение от {message.From} с темой: {message.Subject}\n");
        stringBuilder.Append($"Сожержание: {message.TextBody}");
        
        return stringBuilder.ToString();
    }
}