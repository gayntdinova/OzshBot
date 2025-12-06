using OzshBot.Domain.Entities;

namespace OzshBot.Domain.ValueObjects;

public class ChildInfo
{
    public EducationInfo EducationInfo { get; set; }
    public int? Group { get; set; }
    public List<Session> Sessions { get; init; } = [];
    public List<ContactPerson> ContactPeople { get; init; } = [];
}