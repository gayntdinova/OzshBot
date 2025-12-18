using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.DtoModels;
using OzshBot.Application.ToolsInterfaces;

namespace OzshBot.Infrastructure.Parser;

public class GoogleDocParser: ITableParser
{
    public async Task<Result<ChildDto[]>> GetChildrenAsync(string url)
    {
        try
        {
            var reader = new GoogleDocsReader(url);
            var data = await reader.ReadGoogleSheet();
            return TableParser.GetChildrenAsync(data);
        }
        catch (ArgumentException)
        {
            return Result.Fail(new IncorrectUrlError(url));
        }
    }
}