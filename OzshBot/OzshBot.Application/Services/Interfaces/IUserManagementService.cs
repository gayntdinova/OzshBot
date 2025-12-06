using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserManagementService
{
    public Task<Result<User>> EditUser(User user);
    public Task<Result<User>> AddUserAsync<T>(T user) where T: UserDtoModel;
    public Task<Result> DeleteUserAsync(string phoneNumber);

    public Task<Result> LoadTable(string link);
}