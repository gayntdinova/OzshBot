using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserManagementService
{
    public Task<Result<User>> EditCounsellorAsync(CounsellorDto user);
    public Task<Result<User>> AddCounsellorAsync(CounsellorDto user);
    public Task<Result> DeleteCounsellorAsync(CounsellorDto user);
    
    public Task<Result<User>> EditChildAsync(ChildDto user);
    public Task<Result<User>> AddChildAsync(ChildDto user);
    public Task<Result> DeleteChildAsync(ChildDto user);

    public Task<Result> LoadTable(string link);
}