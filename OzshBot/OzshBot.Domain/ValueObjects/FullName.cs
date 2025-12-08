namespace OzshBot.Domain.ValueObjects;

public class FullName
{
    public string? Surname { get; set; }
    public string? Name { get; set; }
    public string? Patronymic { get; set; }
    public FullName(string? surname=null, string? name=null,  string? patronymic=null)
    {
        Name = name;
        Surname = surname;
        Patronymic = patronymic;
    }
}