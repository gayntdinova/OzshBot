namespace OzshBot.Domain.ValueObjects;

public class FullName
{
    public required string? Name { get; set; }
    public required string? Surname { get; set; }
    public required string? Patronymic { get; set; }
}