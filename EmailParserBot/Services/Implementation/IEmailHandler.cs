namespace EmailParserBot.Services.Implementation;

using Contracts;
using MailKit.Net.Imap;

public interface IEmailHandler
{
    Task HandleNewEmailsAsync();
    Task DisconnectAsync();
}