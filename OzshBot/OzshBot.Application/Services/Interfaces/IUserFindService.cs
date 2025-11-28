using FluentResults;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserFindService
{
    public Task<Result<User[]>> FindUsersByClassAsync(int classNumber);
    public Task<Result<User[]>> FindUsersByGroupAsync(int group);
    public Task<Result<User>> FindUserByTgAsync(TelegramInfo telegramInfo);
    public Task<Result<User[]>> FindUserAsync(string input);
}