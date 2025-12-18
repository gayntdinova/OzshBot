using FluentResults;

namespace OzshBot.Application.AppErrors;

public class IncorrectRowError: Error
{
    public int Row { get; }
    public IncorrectRowError(int row, string message="can't parse row"): base(message)
    {
        Row = row;
        WithMetadata("type", "IncorrectRowError");
    }
}