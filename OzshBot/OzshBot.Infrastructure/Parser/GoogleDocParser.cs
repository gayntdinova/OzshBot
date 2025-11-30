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
    private string url;
    private readonly string applicationName = "Ozsh Bot";

    private void CheckUrl(string url)
    {
        if (url.StartsWith("https://docs.google.com/spreadsheets/d/"))
        {
            this.url = url;
        }
        else throw new ArgumentException("Invalid URL");
    }
    private string GetSpreadsheetIdFromUrl(string url) => url.Split('/')[5];

    private async Task<string> GetPageNameFromUrl(string url, SheetsService service)
    {
        var gid = url.Split('=').Last();
        var spreadsheetId = GetSpreadsheetIdFromUrl(url);
        var spreadsheet = await service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
        var sheet = spreadsheet.Sheets
            .FirstOrDefault(s => s.Properties.SheetId == Int32.Parse(gid));
        return sheet.Properties.Title;
    }

    private GoogleCredential GetCredential(string credentialPath)
    {
        string[] scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        return GoogleCredential.FromFile(credentialPath).CreateScoped(scopes);
    }

    private async Task<IList<IList<object>>> ReadGoodleSheet(string url)
    {
        var credentialPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "core-song-477709-a0-db7e8f5a3e78.json");
        var credential = GetCredential(credentialPath);
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName,
        });

        var spreadsheetId = GetSpreadsheetIdFromUrl(url);
        var range = await GetPageNameFromUrl(url, service);
        
        var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response.Values;
    }

    private ChildDto CreateChildDto(IList<object> row)
    {
        var name = row[0].ToString().Trim().Split();
        var fullName = new FullName(name[0]);
        if (name.Length == 3)
            fullName = new FullName(name[0], name[1], name[2]);
        else if (name.Length == 2)
            fullName = new FullName(name[0], name[1]);
        var eucationInfo = new EducationInfo
        {
            Class = Int32.Parse(row[1].ToString()),
            School = row[3].ToString()
        };
        var childInfo = new ChildInfo
        {
            EducationInfo = eucationInfo
        };
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
        CheckUrl(url);
        var result = new List<ChildDto>();
        var data = await ReadGoodleSheet(url);
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
        return result.ToArray();
    }
}