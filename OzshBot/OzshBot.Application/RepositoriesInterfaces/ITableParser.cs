using FluentResults;
using OzshBot.Application.DtoModels;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface ITableParser
{
    public Task<Result<ChildDto[]?>> GetChildrenAsync(string tableName);
    public Task<Result<CounsellorDto[]?>> GetCounsellorsAsync(string tableName);
}