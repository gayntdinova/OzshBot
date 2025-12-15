using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class Session
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required SessionDates SessionDates { get; set; }
}