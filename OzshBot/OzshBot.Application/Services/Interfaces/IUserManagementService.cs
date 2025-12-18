using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserManagementService
{
    Task<Result<User>> EditUserAsync(User editedUser);
    Task<Result<User>> AddUserAsync<T>(T user) where T: UserDtoModel;
    Task<Result> DeleteUserAsync(string phoneNumber);

    Task<Result> LoadTableAsync(string link, SessionDates sessionDates);
}