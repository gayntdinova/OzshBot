using OzshBot.Domain.Entities;

namespace OzshBot.Domain.ValueObjects;

public class ChildInfo
{
    public EducationInfo EducationInfo { get; set; }
    public int? Group { get; set; }
    public Session[] Sessions { get; init; } = [];
    public ContactPerson[] ContactPeople { get; init; } = [];
}