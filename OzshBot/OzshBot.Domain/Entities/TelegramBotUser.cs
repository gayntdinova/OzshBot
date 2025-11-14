using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class TelegramBotUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public TelegramInfo TelegramBotInfo { get; set; }
    public AccessRights AccessRights { get; set; }
}