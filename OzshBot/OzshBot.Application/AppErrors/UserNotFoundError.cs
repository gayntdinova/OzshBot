using FluentResults;

namespace OzshBot.Application.AppErrors;

public class UserNotFoundError: Error
{
    public UserNotFoundError(string message="user not found") : base(message)
    {
        WithMetadata("type", "NotFound");
    }
}