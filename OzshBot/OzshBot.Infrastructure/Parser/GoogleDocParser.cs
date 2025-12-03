using System.Text.RegularExpressions;
using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using OzshBot.Application;

public class GoogleDocParser: ITableParser
{
    private readonly string applicationName = "Ozsh Bot";

    private string GetSpreadsheetIdFromUrl(string url) => url.Split('/')[5];

    private async Task<string> GetPageNameFromUrl(SheetsService service, string url)
    {
        var gid = url.Split('=').Last();
        var spreadsheetId = GetSpreadsheetIdFromUrl(url);
        var spreadsheet = await service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
        var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.SheetId == Int32.Parse(gid));
        return sheet.Properties.Title;
    }

    private GoogleCredential GetCredential(string fileName)
    {
        var credentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        string[] scopes = [SheetsService.Scope.SpreadsheetsReadonly];
        return GoogleCredential.FromFile(credentialPath).CreateScoped(scopes);
    }

    private async Task<IList<IList<object>>> ReadGoogleSheet(string url)
    {
        var credential = GetCredential("core-song-477709-a0-db7e8f5a3e78.json");
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName,
        });

        var spreadsheetId = GetSpreadsheetIdFromUrl(url);
        var range = await GetPageNameFromUrl(service, url);
        
        var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response.Values;
    }

    private FullName GetFullName(string nameInfo)
    {
        var name = nameInfo.Trim().Split();
        if (name.Length == 3)
            return new FullName(name[0], name[1], name[2]);
        if (name.Length == 2)
            return new FullName(name[0], name[1]);
        return new FullName(name[0]);
    }
    
    public static List<string> ExtractAllPhones(string input)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(input))
            return result;
        
        var matches = Regex.Matches(input, @"[+\d][\d\-\(\)\s]{8,20}");

        foreach (Match m in matches)
        {
            var raw = m.Value;
            var digits = new string(raw.Where(char.IsDigit).ToArray());
            
            if (digits.Length == 11 && (digits.StartsWith("7") || digits.StartsWith("8")))
                digits = digits.Substring(1);
            if (digits.Length == 10)
                result.Add(digits);
        }
        return result;
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
        var fullName = GetFullName(row[0].ToString());
        var childInfo = GetChildInfo(row[3].ToString(), Int32.Parse(row[1].ToString()), row[10].ToString());
        var city = row[2].ToString();
        var birthDate = DateOnly.Parse(row[4].ToString());
        var phoneNumber = row[5].ToString();
        var email = row[6].ToString();
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

    public async Task<Result<ChildDto[]>> GetChildrenAsync(string url)
    {
        if (!url.StartsWith("https://docs.google.com/spreadsheets/d/"))
        {
            return Result.Fail<ChildDto[]>("Invalid URL");
        }
        
        var result = new List<ChildDto>();
        var data = await ReadGoogleSheet(url);
        foreach (var row in data.Skip(1))
        {
            try
            {
                result.Add(CreateChildDto(row));
            }
            catch (Exception e)
            {
                continue;
            }
        }
        return Result.Ok(result.ToArray());
    }
}