namespace EmailParserBot.Contracts;

public class DataFromBodyContract
{
    public string Name { get; set; }
    public string ContactInfo { get; set; }

    public string PaymentAmmount { get; set; }

    public string PaymentTime { get; set; }
    public List<string> PhotoLinks { get; set; } = [];
}