namespace EmailParserBot.Options;

public class ConnectToInboxClientOptions
{
    public static string SectionName = "InboxClient";
    
    public string ImapServerUrl { get; set; }
    public int ImapServerPort { get; set; }
}