using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class User
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public TelegramInfo TelegramInfo { get; set; }
    public ChildInfo? ChildInfo { get; set; }
    public CounsellorInfo? CounsellorInfo { get; set; }
    public Role Role { get; set; }
}