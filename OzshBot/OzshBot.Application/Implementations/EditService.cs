using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Application.Interfaces;
using OzshBot.Domain.Entities;

namespace OzshBot.Application.Implementations;

public class EditService: IEditService
{
    public async Task<Result> EditUserAsync(UserDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> AddUserAsync(UserDto user)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> DeleteUserAsync(UserDto user)
    {
        throw new NotImplementedException();
    }
}