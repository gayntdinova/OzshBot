using System.ComponentModel.DataAnnotations;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class User
{
    public Guid Id { get; init; }
    public required FullName FullName { get; set; }
    public TelegramInfo? TelegramInfo { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? City { get; set; }
    [Phone]
    public required string PhoneNumber { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    public ChildInfo? ChildInfo { get; set; }
    public CounsellorInfo? CounsellorInfo { get; set; }
    public Role Role { get; set; }

    public void UpdateBy(User userUpdate)
    {
        FullName = userUpdate.FullName;
        TelegramInfo = userUpdate.TelegramInfo;
        Birthday = userUpdate.Birthday;
        City = userUpdate.City;
        PhoneNumber = userUpdate.PhoneNumber;
        Email = userUpdate.Email;
        ChildInfo = userUpdate.ChildInfo ?? ChildInfo;
        CounsellorInfo = userUpdate.CounsellorInfo ?? CounsellorInfo;
        Role = userUpdate.Role;
    }
    public override string ToString()
    {
        return $"User({FullName}, {Role}, {TelegramInfo}, {PhoneNumber})";
    }
}