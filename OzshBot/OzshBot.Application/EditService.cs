using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Application.Interfaces;

namespace OzshBot.Application;

public class EditService: IEditService
{
    public Task<Result> EditUserAsync<T>(T user)
    {
        throw new NotImplementedException();
    }

    public Task<Result> AddUserAsync<T>(T user)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteUserAsync<T>(T user)
    {
        throw new NotImplementedException();
    }
}