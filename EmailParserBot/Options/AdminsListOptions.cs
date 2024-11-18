namespace EmailParserBot.Options;

public record AdminsListOptions
{
    public static string SectionName = "AdminsList";
    
    public List<int> Admins { get; set; }
}