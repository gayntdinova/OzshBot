using FluentResults;

namespace OzshBot.Application.AppErrors;

public class InvalidDataError: Error
{
    public InvalidDataError(string message="incorrect data") : base(message)
    {
        WithMetadata("type", "InvalidDataError");
    }
}