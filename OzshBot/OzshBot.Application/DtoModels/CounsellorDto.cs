using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class CounsellorDto: Dto
{
    public TelegramInfo TelegramInfo { get; set; }
    public CounsellorInfo CounsellorInfo { get; set; }

    public User ToUser()
    {
        return new User
        {
            Id = default,
            TelegramInfo = TelegramInfo,
            ChildInfo = null,
            CounsellorInfo = CounsellorInfo,
            Role = Role.Counsellor
        };
    }
}