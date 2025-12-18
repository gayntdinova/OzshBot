using OzshBot.Application.DtoModels;
using UserDomain = OzshBot.Domain.Entities.User;
namespace OzshBot.Bot;

public static class UserToDtoExtention
{
    public static ChildDto ToChildDto(this UserDomain user)
    {
        if (user.ChildInfo == null) throw new ArgumentException();
        return new ChildDto
        {
            Id = user.Id,
            FullName = user.FullName,
            TelegramInfo = user.TelegramInfo,
            Birthday = user.Birthday,
            City = user.City,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            ChildInfo = user.ChildInfo
        };
    }

    public static CounsellorDto ToCounsellorDto(this UserDomain user)
    {
        if (user.CounsellorInfo == null) throw new ArgumentException();
        return new CounsellorDto
        {
            Id = user.Id,
            FullName = user.FullName,
            TelegramInfo = user.TelegramInfo,
            Birthday = user.Birthday,
            City = user.City,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            CounsellorInfo = user.CounsellorInfo
        };
    }
}