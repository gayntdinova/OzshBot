namespace OzshBot.Domain.ValueObjects;

public record FullName
{
    public string Surname { get; private init; }
    public string Name { get; private init; }
    public string? Patronymic { get; private init; }

    public FullName(string surname, string name, string? patronymic = null)
    {
        Name = Capitalize(name);
        Surname = Capitalize(surname);
        Patronymic = patronymic != null ? Capitalize(patronymic) : null;
    }

    private static string Capitalize(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        if (str.Length == 1) return str.ToUpper();
        return str[..1].ToUpper() + str[1..].ToLower();
    }
}