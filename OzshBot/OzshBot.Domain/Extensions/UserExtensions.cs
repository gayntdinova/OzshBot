using OzshBot.Domain.Entities;

namespace OzshBot.Domain.Extensions;

public static class UserExtensions
{
    public static User Clone(this User user)
    {
        return new User
        {
            Id = user.Id,
            FullName = user.FullName with {},
            TelegramInfo = user.TelegramInfo != null
                ? user.TelegramInfo with {}
                : null,
            Birthday = user.Birthday,
            City = user.City,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            ChildInfo = user.ChildInfo?.Clone(),
            CounsellorInfo = user.CounsellorInfo?.Clone(),
            Role = user.Role,
        };
    }
}