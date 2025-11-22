using FluentResults;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IFindService
{
    internal Task<Result<User>> FindUserByTgAsync(TelegramInfo telegramInfo);
    internal Task<User[]?> FindUsersByFullNameAsync(FullName fullName);
    internal Task<User[]?> FindUsersByTownAsync(string town);
    public Task<User[]> FindUsersByClassAsync(int classNumber);
    public Task<User[]> FindUsersByGroupAsync(int group);
    public Task<User[]?> FindUserAsync(string target);
}