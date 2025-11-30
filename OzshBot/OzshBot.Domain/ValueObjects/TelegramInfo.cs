namespace OzshBot.Domain.ValueObjects;

public class TelegramInfo
{
    public required string TgUsername { get; set; }
    public long? TgId { get; set; }
}