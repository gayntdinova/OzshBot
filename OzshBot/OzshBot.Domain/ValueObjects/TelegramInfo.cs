namespace OzshBot.Domain.ValueObjects;

public class TelegramInfo (string tgUsername, long? tgId = null)
{
    public required string TgUsername { get; set; } = tgUsername;
    public long? TgId { get; set; } = tgId;

    public override string ToString()
    {
        var dataString = TgUsername;
        if (TgId != null) dataString += $", {TgId}";
        return $"TelegramInfo({dataString})";
    }
}