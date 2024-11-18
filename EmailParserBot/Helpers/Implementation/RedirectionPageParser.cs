namespace EmailParserBot.Helpers.Implementation;

using Abstract;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;

public class RedirectionPageParser : IRedirectionPageParser
{
    private readonly ILogger<RedirectionPageParser> _logger;

    public RedirectionPageParser(ILogger<RedirectionPageParser> logger)
    {
        _logger = logger;
    }

    public async Task<string?> GetPhotoNameByUrlAsync(string redirectingPageUrl)
    {
        HttpClient httpClient = new HttpClient();
        try
        {
            string responseHtml = await httpClient.GetStringAsync(redirectingPageUrl);
            return await GetPhotoUrlByHtmlBodyAsync(responseHtml);
        }
        catch (Exception exception)
        {
            if (exception is NullReferenceException)
            {
                _logger.LogError("No photo name found from redirect page" + exception);
                return null;
            }
            _logger.LogError(exception.Message);
            throw;
        }
    }

    private async Task<string?> GetPhotoUrlByHtmlBodyAsync(string htmlBody)
    {
        HtmlParser parser = new HtmlParser();
        IHtmlDocument document = await parser.ParseDocumentAsync(htmlBody);
        IAttr tagContent = document.QuerySelector("a")?.Attributes.GetNamedItem("href") 
                        ?? throw new NullReferenceException();
        return tagContent.Value;
    }
}