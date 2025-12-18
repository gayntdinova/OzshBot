using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;

namespace OzshBot.Infrastructure.Parser;

public class GoogleDocsReader
{
    private readonly string applicationName = "Ozsh Bot";
    private readonly string url;

    public GoogleDocsReader(string url)
    {
        if (!url.StartsWith("https://docs.google.com/spreadsheets/d/"))
            throw new ArgumentException("Invalid URL");
        this.url = url;
    }
    private string GetSpreadsheetIdFromUrl() => url.Split('/')[5];

    private async Task<string> GetPageNameFromUrl(SheetsService service)
    {
        var gid = url.Split('=').Last();
        var spreadsheetId = GetSpreadsheetIdFromUrl();
        var spreadsheet = await service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
        var sheet = spreadsheet.Sheets.FirstOrDefault(s => s.Properties.SheetId == Int32.Parse(gid));
        return sheet.Properties.Title;
    }

    private GoogleCredential GetCredential(string fileName)
    {
        var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        string[] scopes = [SheetsService.Scope.SpreadsheetsReadonly];
        return GoogleCredential.FromFile(fullPath).CreateScoped(scopes);
    }

    public async Task<IList<IList<object>>> ReadGoogleSheet()
    {
        var credential = GetCredential("core-song-477709-a0-db7e8f5a3e78.json");
        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = applicationName,
        });

        var spreadsheetId = GetSpreadsheetIdFromUrl();
        var range = await GetPageNameFromUrl(service);
        
        var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response.Values;
    }
}