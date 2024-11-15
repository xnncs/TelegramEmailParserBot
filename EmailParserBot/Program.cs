
using Coravel;
using EmailParserBot.Contracts;
using EmailParserBot.Handlers;
using EmailParserBot.Handlers.Email.Abstract;
using EmailParserBot.Handlers.Email.Implementation;
using EmailParserBot.Helpers.Abstract;
using EmailParserBot.Helpers.Implementation;
using EmailParserBot.Services.Abstract;
using EmailParserBot.Services.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramUpdater;
using TelegramUpdater.Hosting;

IHost app = Host.CreateDefaultBuilder(args)
                 .ConfigureHostConfiguration(configuration =>
                  {
                      IPathHelper pathHelper = new PathHelper();
                      
                      const string appsettingsFilePath = @"appsettings.json";

                      configuration.SetBasePath(pathHelper.GetProjectDirectoryPath())
                                   .AddJsonFile(appsettingsFilePath);
                  })
                 .ConfigureServices((hostContext, services) =>
                  {
                      services.Configure<AuthenticateToEmailContract>(hostContext.Configuration.GetSection(nameof(AuthenticateToEmailContract)));
                      services.Configure<ConnectToInboxClientContract>(hostContext.Configuration.GetSection(nameof(ConnectToInboxClientContract)));
                      services.Configure<AdminsListContract>(hostContext.Configuration.GetSection(nameof(AdminsListContract)));
                      
                      services.AddSingleton<IEmailHandler, ImapEmailHandler>(provider =>
                      {
                          AuthenticateToEmailContract authenticateToEmailContract = provider.GetRequiredService<IOptions<AuthenticateToEmailContract>>().Value;
                          ConnectToInboxClientContract connectToInboxClientContract = provider.GetRequiredService<IOptions<ConnectToInboxClientContract>>().Value;
                          
                          ILogger<ImapEmailHandler> logger = provider.GetRequiredService<ILogger<ImapEmailHandler>>();
                          IEmailService emailService = provider.GetRequiredService<IEmailService>();

                          return new ImapEmailHandler(logger, emailService, authenticateToEmailContract, connectToInboxClientContract);
                      });

                      services.AddScheduler();
                      
                      services.AddScoped<IEmailService, EmailService>();
                      
                      ConfigureTelegramUpdater(services, hostContext.Configuration);
                      
                  }).Build();

app.Services.UseScheduler(scheduler =>
{
    IEmailHandler emailHandler = app.Services.GetService<IEmailHandler>()
        ?? throw new NullReferenceException("Email handler not configured");

    scheduler.Schedule(EmailHandlingAction).EverySeconds(10);
    return;

    async void EmailHandlingAction() => 
        await emailHandler.HandleNewEmailsAsync();
});

await app.RunAsync();
return;

void ConfigureTelegramUpdater(IServiceCollection services, IConfiguration configuration)
{
    string token = configuration.GetSection("TelegramBotToken").Value ??
                throw new Exception("Server error: no telegram bot token configured");

    TelegramBotClient client = new TelegramBotClient(token);

    services.AddHttpClient("TelegramBotClient").AddTypedClient<ITelegramBotClient>(httpClient => client);

    UpdaterOptions updaterOptions = new UpdaterOptions(10,
        allowedUpdates: [UpdateType.Message, UpdateType.CallbackQuery]);

    services.AddTelegramUpdater(client, updaterOptions, botBuilder =>
    {
        botBuilder.AddDefaultExceptionHandler()
                  .AddScopedUpdateHandler<ScopedMessageHandler, Message>();
    });
}