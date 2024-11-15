namespace EmailParserBot.Services.Abstract;

using Contracts;
using Handlers.Email.Abstract;
using Implementation;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

public class ImapEmailHandler : IEmailHandler
{
    private readonly ImapClient _imapClient;
    
    private readonly IEmailService _emailService;
    private readonly ILogger<ImapEmailHandler> _logger;

    private readonly AuthenticateToEmailContract _emailContract;
    private readonly ConnectToInboxClientContract _inboxClientContract;

    public ImapEmailHandler(ILogger<ImapEmailHandler> logger, IEmailService emailService, AuthenticateToEmailContract emailContract, ConnectToInboxClientContract inboxClientContract)
    {
        _logger = logger;
        _emailService = emailService;
        _emailContract = emailContract;
        _inboxClientContract = inboxClientContract;

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
            imapClient.Connect(_inboxClientContract.ImapServerUrl, _inboxClientContract.ImapServerPort, SecureSocketOptions.SslOnConnect);
            imapClient.Authenticate(_emailContract.Email, _emailContract.Password);
            
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