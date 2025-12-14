namespace OzshBot.Domain.ValueObjects;

public record TelegramInfo
{
    public required string TgUsername { get; init; }
    public long? TgId { get; init; }

    public override string ToString()
    {
        var tgIdString = TgId == null ? "?" : TgId.ToString();
        return $"TelegramInfo({TgUsername}, {tgIdString})";
    }
}