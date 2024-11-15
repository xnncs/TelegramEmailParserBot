namespace EmailParserBot.Handlers.Email.Abstract;

using MailKit;
using MailKit.Net.Imap;

public interface IEmailService
{
    Task OnEmailAsync(IMessageSummary messageSummary, ImapClient imapClient);
}