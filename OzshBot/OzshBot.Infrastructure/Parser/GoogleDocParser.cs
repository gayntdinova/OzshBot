namespace OzshBot.Infrastructure;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;


public class GoogleDocParser
{
    private readonly string url;
    private readonly string applicationName = "Ozsh Bot";
    
    public GoogleDocParser(string url)
    {
        if (url.StartsWith("https://docs.google.com/spreadsheets/d/"))
        {
            this.url = url;
        }
        else throw new ArgumentException("Invalid URL");
    }

    public string GetSpreadsheetIdFromUrl(string url) => url.Split('/')[5];
    
    public async Task<string> GetPageNameFromUrl(string url, SheetsService service)
    {
        var gid = url.Split('=').Last();
        var spreadsheetId = GetSpreadsheetIdFromUrl(url);
        var spreadsheet = await service.Spreadsheets.Get(spreadsheetId).ExecuteAsync();
        var sheet = spreadsheet.Sheets
            .FirstOrDefault(s => s.Properties.SheetId == Int32.Parse(gid));
        return sheet.Properties.Title;
    }

    public GoogleCredential GetCredential(string credentialPath)
    {
        string[] scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        return GoogleCredential.FromFile(credentialPath).CreateScoped(scopes);
    }

    public async Task<IList<IList<object>>> ReadGoodleSheet(string url)
    {
        //это потом поменяешь
        var credentialPath = Path.GetFullPath("core-song-477709-a0-db7e8f5a3e78.json");
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
}