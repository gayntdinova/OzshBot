using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;

namespace OzshBot.Application.Services.Interfaces;

public interface IEditService
{
    public Task<Result<User>> EditCounsellorAsync(CounsellorDto user);
    public Task<Result<User>> AddCounsellorAsync(CounsellorDto user);
    public Task<Result<User>> DeleteCounsellorAsync(CounsellorDto user);
    public Task<Result<User>> EditChildAsync(ChildDto user);
    public Task<Result<User>> AddChildAsync(ChildDto user);
    public Task<Result<User>> DeleteChildAsync(ChildDto user);
}