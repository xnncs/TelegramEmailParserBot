namespace EmailParserBot.Helpers.Implementation;

using System.Text.RegularExpressions;
using Abstract;
using Contracts;

public class EmailBodyParser : IEmailBodyParser
{
    public DataFromBodyContract ParseBody(string body)
    {
        DataFromBodyContract contract = new DataFromBodyContract();
        
        Match nameMatch = Regex.Match(body, @"Name:\s*(.*)", RegexOptions.IgnoreCase);
        if (nameMatch.Success)
        {
            contract.Name = nameMatch.Groups[1].Value.Trim();
        }
        
        Match contactMatch = Regex.Match(body, @"Контакты_для_обратной_связи__Телефон_который_есть_в_вотс_апп_или_телеграмм_либо_электронную_почту_:\s*(\S+)", RegexOptions.IgnoreCase);
        if (contactMatch.Success)
        {
            contract.ContactInfo = contactMatch.Groups[1].Value.Trim();
        }
        
        Match paymentAmountRegex = Regex.Match(body, @"Payment amount:\s*(.*)");
        if (paymentAmountRegex.Success)
        {
            contract.PaymentAmmount = paymentAmountRegex.Groups[1].Value.Trim();
        }
        
        Match paymentTimeRegex = Regex.Match(body, @"Payment time:\s*(.*)");
        if (paymentTimeRegex.Success)
        {
            contract.PaymentTime = paymentTimeRegex.Groups[1].Value.Trim();
        }
        
        MatchCollection photoMatches = Regex.Matches(body, @"Две_-_три_фотографии_остатка_с_разных_ракурсов__\d:\s*(https?://\S+)", RegexOptions.IgnoreCase);
        foreach (Match match in photoMatches)
        {
            contract.PhotoLinks.Add(match.Groups[1].Value.Trim());
        }
        
        return contract;
    }
}