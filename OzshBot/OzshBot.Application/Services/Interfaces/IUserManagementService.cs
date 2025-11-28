using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserManagementService
{
    public Task<Result<User>> EditUser(TelegramInfo telegramInfo, User user);
    public Task<Result<User>> AddUser<T>(T user) where T: Dto;
    public Task<Result> DeleteUserAsync(TelegramInfo telegramInfo);

    public Task<Result> LoadTable(string link);
}