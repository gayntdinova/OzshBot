using OzshBot.Domain.Entities;

namespace OzshBot.Domain.ValueObjects;

public class ChildInfo
{
    public EducationInfo? EducationInfo { get; set; }
    public int? Group { get; set; }
    public HashSet<Session> Sessions { get; init; } = [];
    public HashSet<ContactPerson> ContactPeople { get; init; } = [];
}