using System.Text.RegularExpressions;
using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;
using OzshBot.Application.AppErrors;

namespace OzshBot.Infrastructure;

public class TableParser
{
    private readonly Dictionary<string, int> columnIndexes = new Dictionary<string, int>();
    private readonly List<string> requiredColumnNames = new()
    {
        "фио", "класс", "город", "школа", "день рождения",
        "телефон", "email", "комментарий", "статус заявки на сайте"
    };

    private FullName GetFullNameFromString(string nameInfo)
    {
        var name = nameInfo.Trim().Split();
        if (name.Length == 3)
            return new FullName(name[0], name[1], name[2]);
        if (name.Length == 2)
            return new FullName(name[0], name[1]);
        return new FullName(name[0]);
    }
    
    private static List<string> ExtractAllPhones(string input)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(input))
            return result;
        
        var matches = Regex.Matches(input, @"[+\d][\d\-\(\)\s]{8,20}");

        foreach (Match m in matches)
        {
            try
            {
                var phone = NormalizePhone(m.Value);
                result.Add(phone);
            }
            catch (ArgumentException)
            {
                continue;
            }
        }
        return result;
    }
    
    static string NormalizePhone(string input)
    {
        string digits = Regex.Replace(input, @"\D", "");
        
        if (digits.Length == 11)
        {
            digits = digits.Substring(1);
            return "+7" + digits;
        }

        throw new ArgumentException("Invalid phone number");
    }

    private List<ContactPerson> GetContactPeople(string comment)
    {
        var phoneNumbers = ExtractAllPhones(comment);
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

    private ChildDto CreateChildDto(IList<object> row)
    {
        var fullName = GetFullNameFromString(row[columnIndexes["фио"]].ToString());
        var childInfo = GetChildInfo(row[columnIndexes["школа"]].ToString().ToLower(), 
            Int32.Parse(row[columnIndexes["класс"]].ToString()), row[columnIndexes["комментарий"]].ToString());
        var city = row[columnIndexes["город"]].ToString().ToLower();
        var birthDate = DateOnly.Parse(row[columnIndexes["день рождения"]].ToString());
        var phoneNumber = row[columnIndexes["телефон"]].ToString();
        var email = row[columnIndexes["email"]].ToString();
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

    private bool IsRowEmpty(IList<object> row)
    {
        foreach (var cell in row)
        {
            if (cell is string s)
            {
                if (!string.IsNullOrWhiteSpace(s))
                    return false;
            }
            else if (cell != null)
            {
                return false;
            }
        }
        return true;
    }

    private void GetColumnsIndexes(IList<object> row)
    {
        var columnNames = row.Select(r => r.ToString()?.Trim().ToLower()).ToList();
        foreach (var name in columnNames)
        {
            //так нельзя делать, но пока только так
            if (name is null)
                continue;
            columnIndexes[name] = columnNames.IndexOf(name);
        }
    }

    public async Task<Result<ChildDto[]>> GetChildrenAsync(IList<IList<object>> data)
    {
        GetColumnsIndexes(data[0]);
        if (requiredColumnNames.Any(name => !columnIndexes.ContainsKey(name)))
            return Result.Fail(new IncorrectTableFormatError());
        
        var errors = new List<Error>();
        var result = new List<ChildDto>();
        for(var i = 1; i < data.Count; i++)
        {
            try
            {
                var row = data[i];
                if (IsRowEmpty(row)) continue;
                if (row.Count == 12)
                {
                    if (row[11].ToString()?.Trim() == "Отклонена" || 
                        row[columnIndexes["статус заявки на сайте"]].ToString()?.Trim() == "Рассматривается")
                        continue;
                }
                result.Add(CreateChildDto(row));
            }
            catch (Exception e)
            {
                var str = String.Join(", ", data[i].Select(o => o.ToString()));
                errors.Add(new IncorrectRowError(i + 1));
            }
        }
        if (errors.Count > 0)
            return Result.Fail(errors);
        return Result.Ok(result.ToArray());
    }
}