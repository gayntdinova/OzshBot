using FluentResults;

namespace OzshBot.Application.AppErrors;

public class InvalidDataError: Error
{
    public InvalidDataError(string message="") : base(message)
    {
        WithMetadata("type", "InvalidDataError");
    }
}