using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface IUserRepository
{
    Task<User?> FindUserByTgAsync(TelegramInfo telegramInfo);
    Task<User[]?> FindUsersByFullNameAsync(FullName fullName);
    Task<User[]?> FindUsersByTownAsync(string town);
    Task<User[]?> FindUsersByClassAsync(int classNumber);
    Task<User[]?> FindUsersByGroupAsync(int group);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(TelegramInfo telegramInfo);
}