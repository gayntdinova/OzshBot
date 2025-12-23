using System.Globalization;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Infrastructure.Parser;

public class ChildInfoParser
{
    private readonly Dictionary<string, int> columnIndexes;
    private readonly List<string> requiredColumnNames = new()
    {
        "фио", "класс", "город", "школа", "день рождения",
        "телефон", "email", "комментарий"
    };

    public ChildInfoParser(Dictionary<string, int> columnIndexes)
    {
        if (requiredColumnNames.Any(name => !columnIndexes.ContainsKey(name)))
            throw new InvalidOperationException("Неверный формат таблицы");
        this.columnIndexes = columnIndexes;
    }
    private FullName GetFullNameFromString(string? nameInfo)
    {
        if (nameInfo == null)
            throw new ArgumentException("имени нет");
        var name = nameInfo.Trim().Split();
        if (name.Length == 3)
            return new FullName(name[0], name[1], name[2]);
        if (name.Length == 2)
            return new FullName(name[0], name[1]);
        throw new ArgumentException("неверный формат имени");
    }

    private HashSet<ContactPerson> GetContactPeople(string? comment)
    {
        if (comment == null) return new HashSet<ContactPerson>();
        var phoneNumbers = PhoneParser.ExtractAllPhones(comment);
        var contactPeople = phoneNumbers.Select(number
            => new ContactPerson { PhoneNumber = number, FullName = new FullName("-", "-") }).ToHashSet();
        return contactPeople;
    }

    private ChildInfo GetChildInfo(string? school, string? grade, string? comment, string? group)
    {
        int intGrade;
        if (grade == null || school == null || !Int32.TryParse(grade, out intGrade))
            throw new ArgumentException();
        var educationInfo = new EducationInfo
        {
            Class = intGrade,
            School = school.ToLower()
        };
        var contactPeople = GetContactPeople(comment);
        if (group != null && Int32.TryParse(group, out var intGroup))
            return new ChildInfo { EducationInfo = educationInfo, ContactPeople = contactPeople, Group = intGroup };
        return new ChildInfo { EducationInfo = educationInfo, ContactPeople = contactPeople, Group = null };
    }

    private DateOnly? GetBirthDate(List<string?> row)
    {
        string[] formats = { "dd.MM.yyyy", "yyyy-MM-dd" };

        if (DateOnly.TryParseExact(
            row[columnIndexes["день рождения"]].Trim(),
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var birthDate))
        {
            return birthDate;
        }
        else
        {
            return null;
            //Console.WriteLine($"Неверный формат даты: {row[columnIndexes["день рождения"]]}");
        }

    }

    public ChildDto CreateChildDto(List<string?> row)
    {
        string? group = null;
        if (columnIndexes.ContainsKey("отряд"))
            group = row[columnIndexes["отряд"]];
        var childInfo = GetChildInfo(row[columnIndexes["школа"]],
            row[columnIndexes["класс"]], row[columnIndexes["комментарий"]], group);

        var fullName = GetFullNameFromString(row[columnIndexes["фио"]]);

        if (row[columnIndexes["город"]] is null || row[columnIndexes["день рождения"]] is null)
            throw new ArgumentException();
        var city = row[columnIndexes["город"]].ToLower();
        var birthDate = GetBirthDate(row);

        var phoneNumber = PhoneParser.NormalizePhone(row[columnIndexes["телефон"]]);
        var email = row[columnIndexes["email"]];


        return new ChildDto
        {
            FullName = fullName,
            Birthday = birthDate,
            City = city,
            PhoneNumber = phoneNumber,
            Email = email,
            ChildInfo = childInfo
        };
    }
}