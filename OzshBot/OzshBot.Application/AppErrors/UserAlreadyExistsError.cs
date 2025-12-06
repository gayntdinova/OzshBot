using FluentResults;

namespace OzshBot.Application.AppErrors;

public class UserAlreadyExistsError: Error
{
    public UserAlreadyExistsError(string message="User has already been added") : base(message)
    {
        WithMetadata("type", "UserAlreadyExistsError");
    }
}