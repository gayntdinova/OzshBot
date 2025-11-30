namespace OzshBot.Domain.ValueObjects;

public class TelegramInfo
{
    public required string TgUsername { get; set; }
    public long? TgId { get; set; }

    public override string ToString()
    {
        var tgIdString = TgId == null ? "?" : TgId.ToString();
        return $"TelegramInfo({TgUsername}, {tgIdString})";
    }
}