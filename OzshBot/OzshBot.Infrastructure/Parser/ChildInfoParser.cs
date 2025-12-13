using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;
using OzshBot.Application.DtoModels;
namespace OzshBot.Infrastructure;

public class ChildInfoParser
{
    private readonly Dictionary<string, int> columnIndexes;
    private readonly List<string> requiredColumnNames = new()
    {
        "фио", "класс", "город", "школа", "день рождения",
        "телефон", "email", "комментарий", "статус заявки на сайте"
    };

    public ChildInfoParser(Dictionary<string, int> columnIndexes)
    {
        if (requiredColumnNames.Any(name => !columnIndexes.ContainsKey(name)))
            throw new InvalidOperationException("Неверный формат таблицы");
        this.columnIndexes = columnIndexes;
    }
    private FullName GetFullNameFromString(string nameInfo)
    {
        var name = nameInfo.Trim().Split();
        if (name.Length == 3)
            return new FullName(name[0], name[1], name[2]);
        if (name.Length == 2)
            return new FullName(name[0], name[1]);
        return new FullName(name[0]);
    }

    private List<ContactPerson> GetContactPeople(string comment)
    {
        var phoneNumbers = PhoneParser.ExtractAllPhones(comment);
        var contactPeople = phoneNumbers.Select(number
            => new ContactPerson{PhoneNumber = number, FullName = null}).ToList();
        return contactPeople;
    }

    private ChildInfo GetChildInfo(string scool, int grade, string comment)
    {
        var eucationInfo = new EducationInfo
        {
            Class = grade,
            School = scool
        };
        var contactPeople = GetContactPeople(comment);
        return new ChildInfo{ EducationInfo = eucationInfo, ContactPeople = contactPeople };
    }

    public ChildDto CreateChildDto(List<string> row)
    {
        var fullName = GetFullNameFromString(row[columnIndexes["фио"]]);
        var childInfo = GetChildInfo(row[columnIndexes["школа"]].ToLower(), 
            Int32.Parse(row[columnIndexes["класс"]]), row[columnIndexes["комментарий"]]);
        var city = row[columnIndexes["город"]].ToLower();
        var birthDate = DateOnly.Parse(row[columnIndexes["день рождения"]]);
        var phoneNumber = row[columnIndexes["телефон"]];
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