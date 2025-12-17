using FluentResults;

namespace OzshBot.Application.AppErrors;

public class UserAlreadyHasRoleError: Error
{
    public UserAlreadyHasRoleError(string message="user already has role") : base(message)
    {
        WithMetadata("type", "UserAlreadyHasRole");
    }
}