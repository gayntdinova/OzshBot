using FluentResults;

namespace OzshBot.Application.AppErrors;

public class InvalidUrlError: Error
{
    public InvalidUrlError(string message="incorrect url") : base(message)
    {
        WithMetadata("type", "InvalidUrlError");
    }
}