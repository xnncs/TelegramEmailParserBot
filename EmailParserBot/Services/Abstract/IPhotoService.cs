namespace EmailParserBot.Services.Abstract;

public interface IPhotoService
{
    Task<Stream> GetPhotoAsync(string photoPublicUrl);
}