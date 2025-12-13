using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain;

public static class UserExtensions
{
    public static User Clone(this User user)
    {
        return new User
        {
            Id = user.Id,
            FullName = new FullName(user.FullName.Surname, user.FullName.Name, user.FullName.Patronymic),
            TelegramInfo = user.TelegramInfo != null
                ? new TelegramInfo
                {
                    TgUsername = user.TelegramInfo.TgUsername,
                    TgId = user.TelegramInfo.TgId
                }
                : null,
            Birthday = user.Birthday,
            City = user.City,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            ChildInfo = user.ChildInfo != null ? new ChildInfo
                {
                    EducationInfo = user.ChildInfo.EducationInfo != null ? new EducationInfo
                        {
                            Class = user.ChildInfo.EducationInfo.Class,
                            School = user.ChildInfo.EducationInfo.School,
                        }
                        : null,
                    Group = user.ChildInfo.Group,
                    Sessions = user.ChildInfo.Sessions,
                    ContactPeople = user.ChildInfo.ContactPeople,
                }
                : null,
            CounsellorInfo = user.CounsellorInfo != null?
            new CounsellorInfo
            {
                Group = user.CounsellorInfo.Group,
                Sessions = user.CounsellorInfo.Sessions,
            }
            : null,
            Role = user.Role,
        };
    }
}