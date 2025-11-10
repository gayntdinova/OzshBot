namespace OzshBot.Domain.Entities;

public abstract class TelegramUser
{
    public required string TgUsername { get; set; }
    public long? TgId { get; set; }
}