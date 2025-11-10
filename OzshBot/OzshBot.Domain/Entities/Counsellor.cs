using OzshBot.Domain.ValueObjects;
namespace OzshBot.Domain.Entities;

public class Counsellor
{
    public Guid Id { get; init; } = Guid.NewGuid(); 
    public required PersonalInfo PersonalInfo { get; set; }
    public required TelegramInfo TelegramInfo { get; set; }
    public required int? Group { get; set; }
    public required List<Session> Sessions { get; set; }
}