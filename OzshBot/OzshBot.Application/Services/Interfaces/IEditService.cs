using FluentResults;
using OzshBot.Application.DtoModels;

namespace OzshBot.Application.Services.Interfaces;

public interface IEditService
{
    public Task<Result> EditCounsellorAsync(CounsellorDto user);
    public Task<Result> AddCounsellorAsync(CounsellorDto user);
    public Task<Result> DeleteCounsellorAsync(CounsellorDto user);
    public Task<Result> EditChildAsync(ChildDto user);
    public Task<Result> AddChildAsync(ChildDto user);
    public Task<Result> DeleteChildAsync(ChildDto user);
}