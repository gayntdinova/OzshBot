using FluentResults;

namespace OzshBot.Application.AppErrors;

public class InvalidTableFormatError: Error
{
    public InvalidTableFormatError(string message="incorrect table format") : base(message)
    {
        WithMetadata("type", "InvalidTableFormatError");
    }
}