namespace EmailParserBot.Handlers.Email.Abstract;

public interface IEmailHandler
{
    Task HandleNewEmailsAsync();
    Task DisconnectAsync();
}