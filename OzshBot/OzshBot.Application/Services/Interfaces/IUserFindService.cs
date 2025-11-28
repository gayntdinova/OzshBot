using FluentResults;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserFindService
{
    public Task<Result<IEnumerable<User>>> FindUsersByClassAsync(int classNumber);
    public Task<Result<IEnumerable<User>>> FindUsersByGroupAsync(int group);
    public Task<Result<IEnumerable<User>>> FindUserAsync(string input);
    public Task<Result<User>> FindUserByTgAsync(TelegramInfo telegramInfo);
}