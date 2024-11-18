namespace EmailParserBot.Services.Models;

public record GetPublicResourceResponse
{
    public string Href { get; set; }
    public string Method { get; set; }
    public bool Templated { get; set; }
}