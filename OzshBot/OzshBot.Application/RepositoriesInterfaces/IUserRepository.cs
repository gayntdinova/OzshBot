using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface IUserRepository
{
    Task<User?> GetUserByTgAsync(TelegramInfo telegramInfo);
    Task<IEnumerable<User>?> GetUsersByFullNameAsync(FullName fullName);
    Task<IEnumerable<User>?> GetUsersByTownAsync(string town);
    Task<IEnumerable<User>?> GetUsersByClassAsync(int classNumber);
    Task<IEnumerable<User>?> GetUsersByGroupAsync(int group);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(TelegramInfo telegramInfo);
}