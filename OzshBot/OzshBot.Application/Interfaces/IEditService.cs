using FluentResults;
using OzshBot.Application.DtoModels;

namespace OzshBot.Application.Interfaces;

public interface IEditService
{
    public Task<Result> EditUserAsync(UserDto user);
    public Task<Result> AddUserAsync(UserDto user);
    public Task<Result> DeleteUserAsync(UserDto user);
}