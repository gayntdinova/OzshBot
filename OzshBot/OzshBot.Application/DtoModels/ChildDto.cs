using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.DtoModels;

public class ChildDto: IUserDtoModel
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public FullName FullName { get; set; }
    public TelegramInfo TelegramInfo { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? City { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    
    public ChildInfo ChildInfo { get; set; }

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
            ChildInfo = ChildInfo,
            CounsellorInfo = null,
            Role = Role.Child
        };
    }
}