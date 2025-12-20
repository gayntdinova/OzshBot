using FluentResults;

namespace OzshBot.Application.AppErrors;

public class IncorrectDataError: Error
{
    public IncorrectDataError(string message="incorrect data") : base(message)
    {
        WithMetadata("type", "IncorrectDataError");
    }
}