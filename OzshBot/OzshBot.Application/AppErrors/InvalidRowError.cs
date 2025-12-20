using FluentResults;

namespace OzshBot.Application.AppErrors;

public class InvalidRowError: Error
{
    public int Row { get; }
    public InvalidRowError(int row, string message="can't parse row"): base(message)
    {
        Row = row;
        WithMetadata("type", "IncorrectRowError");
    }
}