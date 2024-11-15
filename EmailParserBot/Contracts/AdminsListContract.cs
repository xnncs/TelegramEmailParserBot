namespace EmailParserBot.Contracts;

public record AdminsListContract
{
    public List<int> Admins { get; set; }
}