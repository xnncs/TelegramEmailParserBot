namespace EmailParserBot.Handlers.Email.Implementation;

using EmailParserBot.Contracts;
using EmailParserBot.Handlers.Email.Abstract;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Options;
using Services.Abstract;

public class ImapEmailHandler : IEmailHandler
{
    private readonly ImapClient _imapClient;
    
    private readonly IEmailService _emailService;
    private readonly ILogger<ImapEmailHandler> _logger;

    private readonly AuthenticateToEmailOptions _emailOptions;
    private readonly ConnectToInboxClientOptions _inboxClientOptions;

    public ImapEmailHandler(ILogger<ImapEmailHandler> logger, IEmailService emailService, AuthenticateToEmailOptions emailOptions, ConnectToInboxClientOptions inboxClientOptions)
    {
        _logger = logger;
        _emailService = emailService;
        _emailOptions = emailOptions;
        _inboxClientOptions = inboxClientOptions;

        _imapClient = Connect();
    }

    public async Task HandleNewEmailsAsync()
    {
        IMailFolder? inbox = _imapClient.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadWrite);
            
        // Получаем все непрочитанные сообщения
        IList<IMessageSummary>? newMessages = await inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId | MessageSummaryItems.Flags);
            
        // Фильтруем только непрочитанные сообщения
        IEnumerable<IMessageSummary> unreadMessages = newMessages.Where(m => m.Flags != MessageFlags.Seen);
        
        foreach (IMessageSummary messageSummary in unreadMessages)
        {
            await _emailService.OnEmailAsync(messageSummary, _imapClient);
        }
    }

    private ImapClient Connect()
    {
        ImapClient imapClient = new ImapClient();
        try
        {
            imapClient.Connect(_inboxClientOptions.ImapServerUrl, _inboxClientOptions.ImapServerPort, SecureSocketOptions.SslOnConnect);
            imapClient.Authenticate(_emailOptions.Email, _emailOptions.Password);
            
            _logger.LogInformation("Connected to inbox server");
            
            return imapClient;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to inbox server");
            throw;
        }
    }

    public async Task DisconnectAsync()
    {
        await _imapClient.DisconnectAsync(true);
        _logger.LogInformation("Disconnected from inbox server");
    }
}