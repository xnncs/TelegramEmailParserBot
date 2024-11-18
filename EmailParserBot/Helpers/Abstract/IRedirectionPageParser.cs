namespace EmailParserBot.Helpers.Abstract;

public interface IRedirectionPageParser
{
    Task<string?> GetPhotoNameByUrlAsync(string redirectingPageUrl);
}