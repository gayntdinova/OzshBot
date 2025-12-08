using FluentResults;
using OzshBot.Application.DtoModels;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface ITableParser
{
    Task<Result<ChildDto[]>> GetChildrenAsync(string url);
}