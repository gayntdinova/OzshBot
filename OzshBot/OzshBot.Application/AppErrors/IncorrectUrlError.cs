using FluentResults;

namespace OzshBot.Application.AppErrors;

public class IncorrectUrlError: Error
{
    public IncorrectUrlError(string message="incorrect url") : base(message)
    {
        WithMetadata("type", "IncorrectUrlError");
    }
}