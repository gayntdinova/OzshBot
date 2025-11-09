using OzshBot.Domain.ValueObjects;

namespace OzshBot.Domain.Entities;

public class Child: Person
{
    public required int Class { get; set; }
    public required int? Group { get; set; }
    public required string Town { get; set; }
    public required string School { get; set; }
    public required List<Session> Sessions { get; set; }
    public required List<Parent>? Parents { get; set; }
}