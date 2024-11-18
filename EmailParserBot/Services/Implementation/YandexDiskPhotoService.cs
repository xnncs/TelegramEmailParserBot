namespace EmailParserBot.Services.Implementation;

using System.Text.Json;
using Abstract;
using Microsoft.Extensions.Options;
using Models;
using Options;

public class YandexDiskPhotoService : IPhotoService
{
    private readonly YandexDiskOptions _diskOptions;

    public YandexDiskPhotoService(IOptions<YandexDiskOptions> diskOptions)
    {
        _diskOptions = diskOptions.Value;
    }

    public async Task<Stream> GetPhotoAsync(string photoPublicUrl)
    {
        HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", _diskOptions.OAuthToken);
        
        HttpResponseMessage fetchResult = await client.GetAsync($"https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key={photoPublicUrl}");

        GetPublicResourceResponse response = await fetchResult.Content.ReadAsAsync<GetPublicResourceResponse>();
            
        HttpResponseMessage downloadResult = await client.GetAsync(response.Href);

        return await downloadResult.Content.ReadAsStreamAsync();
    }
}