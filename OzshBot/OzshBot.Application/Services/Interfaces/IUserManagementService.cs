using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserManagementService
{
    public Task<Result<User>> EditUserAsync(User editedUser, string phoneNumber);
    public Task<Result<User>> AddUserAsync<T>(T user) where T: UserDtoModel;
    public Task<Result> DeleteUserAsync(string phoneNumber);

    public Task<Result> LoadTableAsync(string link);
}