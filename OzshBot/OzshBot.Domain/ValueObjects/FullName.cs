namespace OzshBot.Domain.ValueObjects;

public record FullName
{
    public string Surname { get; init; }
    public string Name { get; init; }
    public string? Patronymic { get; init; }
    public FullName(string surname, string name,  string? patronymic=null)
    {
        Name = name;
        Surname = surname;
        Patronymic = patronymic;
    }
}