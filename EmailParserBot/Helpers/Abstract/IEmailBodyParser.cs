namespace EmailParserBot.Helpers.Abstract;

using Contracts;

public interface IEmailBodyParser
{
    DataFromBodyContract ParseBody(string body);
}