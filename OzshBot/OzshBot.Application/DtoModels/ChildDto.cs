using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class ChildDto: Dto
{
    public TelegramInfo TelegramInfo { get; set; }
    public ChildInfo ChildInfo { get; set; }

    public User ToUser()
    {
        return new User
        {
            Id = default,
            TelegramInfo = TelegramInfo,
            ChildInfo = ChildInfo,
            CounsellorInfo = null,
            Role = Role.Child
        };
    }
}