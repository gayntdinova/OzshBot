using OzshBot.Domain.Enums;

namespace OzshBot.Domain.Entities;

public class Session
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required int Year { get; set; }
    public required Season Season { get; set; }
}