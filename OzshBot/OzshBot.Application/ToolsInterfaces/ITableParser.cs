using FluentResults;
using OzshBot.Application.DtoModels;

namespace OzshBot.Application.ToolsInterfaces;

public interface ITableParser
{
    Task<Result<ChildDto[]>> GetChildrenAsync(string url);
}