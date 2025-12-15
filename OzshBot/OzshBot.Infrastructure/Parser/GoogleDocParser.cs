using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Application.AppErrors;
using OzshBot.Application.ToolsInterfaces;

namespace OzshBot.Infrastructure;

public class GoogleDocParser: ITableParser
{
    public async Task<Result<ChildDto[]>> GetChildrenAsync(string url)
    {
        try
        {
            var reader = new GoogleDocsReader(url);
            var data = await reader.ReadGoogleSheet();
            return new TableParser().GetChildrenAsync(data);
        }
        catch (ArgumentException)
        {
            return Result.Fail(new IncorrectUrlError(url));
        }
    }
}