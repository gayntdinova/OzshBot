using OzshBot.Domain.Entities;

namespace OzshBot.Domain.ValueObjects;

public class CounsellorInfo
{
    public int? Group { get; set; }
    public HashSet<Session> Sessions { get; init; } = [];
}