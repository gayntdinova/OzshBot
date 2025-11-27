namespace OzshBot.Domain.ValueObjects;

public class FullName
{
    public FullName(string? name=null, string? surname=null, string? patronymic=null)
    {
        Name = name;
        Surname = surname;
        Patronymic = patronymic;
    }

    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Patronymic { get; set; }

    public override string ToString()
    {
        var resultString = string.Empty;
        if (Name != null) resultString += $" {Name}";
        if (Surname != null) resultString += $" {Surname}";
        if (Patronymic != null) resultString += $" {Patronymic}";
        return resultString;
    }
}