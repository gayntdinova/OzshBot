namespace OzshBot.Domain.ValueObjects;

public record TelegramInfo
{
    public required string TgUsername { get; init; }
    public long? TgId { get; init; }
}