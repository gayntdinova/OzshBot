using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class ChildDto: UserDtoModel
{
    public ChildInfo ChildInfo { get; set; }

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
            ChildInfo = ChildInfo,
            CounsellorInfo = null,
            Role = Role.Child
        };
    }
}