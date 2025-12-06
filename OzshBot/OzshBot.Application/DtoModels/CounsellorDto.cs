using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class CounsellorDto: UserDtoModel
{
    public CounsellorInfo CounsellorInfo { get; set; }

    public override User ToUser()
    {
        return new User
        {
            Id = Id,
            FullName = FullName,
            TelegramInfo = TelegramInfo,
            Birthday = Birthday,
            City = City,
            PhoneNumber = PhoneNumber,
            Email = Email,
            ChildInfo = null,
            CounsellorInfo = CounsellorInfo,
            Role = Role.Counsellor
        };
    }
}