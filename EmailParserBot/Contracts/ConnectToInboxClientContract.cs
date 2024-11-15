namespace EmailParserBot.Contracts;

public class ConnectToInboxClientContract
{
    public string ImapServerUrl { get; set; }
    public int ImapServerPort { get; set; }
}