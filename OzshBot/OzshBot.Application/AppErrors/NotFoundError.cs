using FluentResults;

namespace OzshBot.Application.AppErrors;

public class NotFoundError: Error
{
    public NotFoundError(string message="user not found") : base(message)
    {
        WithMetadata("type", "NotFound");
    }
}