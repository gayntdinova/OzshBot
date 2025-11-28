using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class CounsellorDto: IUserDtoModel
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public FullName FullName { get; set; }
    public TelegramInfo TelegramInfo { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    
    public CounsellorInfo CounsellorInfo { get; set; }

    public User ToUser()
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