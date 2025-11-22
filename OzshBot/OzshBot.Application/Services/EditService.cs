using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;

namespace OzshBot.Application.Services;

public class EditService: IEditService 
{
    public async Task<Result<User>> EditCounsellorAsync(CounsellorDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<User>> AddCounsellorAsync(CounsellorDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<User>> DeleteCounsellorAsync(CounsellorDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<User>> EditChildAsync(ChildDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<User>> AddChildAsync(ChildDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<User>> DeleteChildAsync(ChildDto user)
    {
        throw new NotImplementedException();
    }
}