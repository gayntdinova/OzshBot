using FluentResults;

namespace OzshBot.Application.AppErrors;

public class SessionNotFoundError: Error
{
    public SessionNotFoundError(string message="session not found") : base(message)
    {
        WithMetadata("type", "NotFound");
    }
}