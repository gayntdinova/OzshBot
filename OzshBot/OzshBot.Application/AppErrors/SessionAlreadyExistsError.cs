using FluentResults;

namespace OzshBot.Application.AppErrors;

public class SessionAlreadyExistsError: Error
{
    public SessionAlreadyExistsError(string message="session has already been added") : base(message)
    {
        WithMetadata("type", "SessionAlreadyExistsError");
    }
}