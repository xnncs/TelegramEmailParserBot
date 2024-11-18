namespace EmailParserBot.Options;

public record AuthenticateToEmailOptions
{
    public static string SectionName = "AuthenticateToEmail";
    
    public string Email { get; set; }
    public string Password { get; set; }
}