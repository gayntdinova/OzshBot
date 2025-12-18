using FluentResults;

namespace OzshBot.Application.AppErrors;

public class SessionIntersectError: Error
{
    public SessionIntersectError(string message="session not found") : base(message)
    {
        WithMetadata("type", "NotFound");
    }
}