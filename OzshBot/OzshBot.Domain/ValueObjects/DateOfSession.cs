namespace OzshBot.Domain.ValueObjects;

public class DateOfSession
{
    public DateOnly SessionStart { get; init; }
    public DateOnly SessionEnd { get; init; }
}