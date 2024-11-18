namespace EmailParserBot.Services.Implementation;

using System.Text;
using Abstract;
using EmailParserBot.Contracts;
using EmailParserBot.Handlers.Email.Abstract;
using Helpers.Abstract;
using MailKit;
using MailKit.Net.Imap;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Options;
using Telegram.Bot;
using Telegram.Bot.Types;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly ITelegramBotClient _botClient;
    
    private readonly AdminsListOptions _adminsOptions;
    
    private readonly IEmailBodyParser _emailBodyParser;
    private readonly IRedirectionPageParser _redirectionPageParser;
    
    private readonly IPhotoService _photoService;

    public EmailService(ILogger<EmailService> logger, ITelegramBotClient botClient, IOptions<AdminsListOptions> admins, IEmailBodyParser emailBodyParser, IRedirectionPageParser redirectionPageParser, IPhotoService photoService)
    {
        _logger = logger;
        _botClient = botClient;
        _emailBodyParser = emailBodyParser;
        _redirectionPageParser = redirectionPageParser;
        _photoService = photoService;
        _adminsOptions = admins.Value;
    }

    public async Task OnEmailAsync(IMessageSummary messageSummary, ImapClient imapClient)
    {
        try
        {
            MimeMessage message = await imapClient.Inbox.GetMessageAsync(messageSummary.UniqueId);
            _logger.LogInformation($"{message.Date.ToString()} Received a message from: {message.From} with subject: {message.Subject} and body:\n{message.TextBody ?? message.HtmlBody}");

            await OnMessageLogicAsync(message);
            await imapClient.Inbox.AddFlagsAsync(messageSummary.UniqueId, MessageFlags.Seen, true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Handled exception, while handling email:" + ex.Message);
        }
    }


    private const string MessageSubject = "New order [istochnik-sily.ru]";
    private async Task OnMessageLogicAsync(MimeMessage message)
    {
        if (message.Subject != MessageSubject)
        {
            return;
        }
        
        DataFromBodyContract data = _emailBodyParser.ParseBody(message.HtmlBody ?? message.TextBody);
        
        string?[] photoNames = await Task.WhenAll(data.PhotoLinks
                                                          .Select(async link => await _redirectionPageParser
                                                                                                .GetPhotoNameByUrlAsync(link)
                                                       ));
        
        Stream[] photosStreams =
            await Task.WhenAll(
                photoNames.Select(async photoName => await _photoService
                   .GetPhotoAsync(photoName)
                ));


        List<InputMediaPhoto> photos = photosStreams.Select(photoStream =>
            new InputMediaPhoto(
                new InputMedia(photoStream, Guid.NewGuid().ToString()
                ))).ToList();

        string responseMessage = GetOutputText(data);
        photos[0].Caption = responseMessage;
        

        foreach (long admin in _adminsOptions.Admins)
        {
            await _botClient.SendMediaGroupAsync(admin, photos);
        }
        
        foreach (Stream photosStream in photosStreams)
        {
            await photosStream.DisposeAsync();
        }
    }

    private string GetOutputText(DataFromBodyContract data)
    {
        return $"""
                Поступил новый заказ от пользователя {data.Name.Replace("<br>", "")}
                Контактная информация: {data.ContactInfo.Replace("<br>", "")}

                Стоимость заказ - {data.PaymentAmmount.Replace("<br>", "")}
                
                Дата оплаты: {data.PaymentTime.Replace("<br>", "")}
                """;
    }
}