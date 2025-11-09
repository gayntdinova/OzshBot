using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public abstract class Person
{
    public Guid Id { get; init; }
    public required FullName FullName { get; set; }
    public required DateTime Birthday { get; set; }
    public required TelegramInfo TelegramInfo { get; set; }
    public required string? PhoneNumber { get; set; }
    public required string? Email { get; set; }

    public Person()
    {
        Id = Guid.NewGuid();
    }

}