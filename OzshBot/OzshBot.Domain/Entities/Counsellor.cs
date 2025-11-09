namespace OzshBot.Domain.Entities;

public class Counsellor: Person
{
    public required int? Group { get; set; }
    public required List<Session> Sessions { get; set; }
}