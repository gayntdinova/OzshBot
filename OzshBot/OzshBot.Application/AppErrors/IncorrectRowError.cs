using FluentResults;

namespace OzshBot.Application.AppErrors;

public class IncorrectRowError: Error
{
    public string Row { get; }
    public IncorrectRowError(string row,string message="can't parse row"): base(message)
    {
        Row = row;
        WithMetadata("type", "IncorrectRowError");
    }
}