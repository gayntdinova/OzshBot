namespace OzshBot.Domain.Entities;

public class Session
{
    public Guid Id { get; init; }
    public required DateTime Date { get; set; }
    public required Season Season { get; set; }
}