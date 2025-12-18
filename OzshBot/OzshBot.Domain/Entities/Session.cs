using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class Session
{
    public Guid Id { get; init; }
    public required SessionDates SessionDates { get; set; }
}