using FluentResults;

namespace OzshBot.Application.AppErrors;

public class IncorrectTableFormatError: Error
{
    public IncorrectTableFormatError(string message="incorrect table format") : base(message)
    {
        WithMetadata("type", "IncorrectUrlError");
    }
}