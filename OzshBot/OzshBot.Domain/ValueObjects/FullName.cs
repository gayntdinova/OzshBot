namespace OzshBot.Domain.ValueObjects;

public class FullName
{
    public string Surname { get; set; }
    public string Name { get; set; }
    public string Patronymic { get; set; }
    public FullName(string surname=null, string name=null,  string patronymic=null)
    {
        Name = name;
        Surname = surname;
        Patronymic = patronymic;
    }

    public override string ToString()
    {
        var resultString = string.Empty;
        if (Surname != null) resultString += $" {Surname}";
        if (Name != null) resultString += $" {Name}";
        if (Patronymic != null) resultString += $" {Patronymic}";
        return resultString;
    }
}