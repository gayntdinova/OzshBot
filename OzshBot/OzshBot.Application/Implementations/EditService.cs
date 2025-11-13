using FluentResults;
using OzshBot.Application.Interfaces;

namespace OzshBot.Application.Implementations;

public class EditService: IEditService
{
    public async Task<Result> EditUserAsync<T>(T user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> AddUserAsync<T>(T user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> DeleteUserAsync<T>(T user)
    {
        throw new NotImplementedException();
    }
}