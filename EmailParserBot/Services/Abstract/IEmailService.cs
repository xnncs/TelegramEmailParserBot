namespace EmailParserBot.Services.Abstract;

using MailKit;
using MailKit.Net.Imap;

public interface IEmailService
{
    Task OnEmailAsync(IMessageSummary messageSummary, ImapClient imapClient);
}