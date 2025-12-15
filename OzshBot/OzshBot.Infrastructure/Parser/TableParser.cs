using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Application.AppErrors;
using OzshBot.Infrastructure.Parser;

namespace OzshBot.Infrastructure;

public class TableParser
{ 
    private static bool IsRowEmpty(IList<object> row)
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

    private static Dictionary<string, int> GetColumnsIndexes(IList<object> row)
    {
        var columnIndexes = new Dictionary<string, int>();
        var columnNames = row.Select(r => r.ToString()?.Trim().ToLower()).ToList();
        foreach (var name in columnNames)
        {
            //так нельзя делать, но пока только так
            if (name is null)
                continue;
            columnIndexes[name] = columnNames.IndexOf(name);
        }
        return columnIndexes;
    }

    public static Result<ChildDto[]> GetChildrenAsync(IList<IList<object>> data)
    {
        ChildInfoParser childInfoParser;
        Dictionary<string, int> columnIndexes;
        try
        {
            columnIndexes = GetColumnsIndexes(data[0]);
            childInfoParser = new ChildInfoParser(columnIndexes);
        }
        catch (InvalidOperationException)
        {
            return Result.Fail(new IncorrectTableFormatError());
        }
        catch (ArgumentOutOfRangeException)
        {
            return Result.Fail(new IncorrectTableFormatError());
        }
        
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

                var stringRow = row.Select(x => x.ToString()).ToList();
                result.Add(childInfoParser.CreateChildDto(stringRow));
            }
            catch (Exception e)
            {
                errors.Add(new IncorrectRowError(i + 1));
            }
        }
        
        if (errors.Count > 0)
            return Result.Fail(errors);
        return Result.Ok(result.ToArray());
    }
}