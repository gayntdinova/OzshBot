using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface IUserRepository
{
    public Task<User?> FindUserByTgAsync(TelegramInfo telegramInfo);
    public Task<User[]?> FindUsersByFullNameAsync(FullName fullName);
    public Task<User[]?> FindUsersByTownAsync(string town);
    public Task<User[]?> FindUsersByClassAsync(int classNumber);
    public Task<User[]?> FindUsersByGroupAsync(int group);
    public Task AddUserAsync(User user);
    public Task EditUserAsync(User user);
    public Task DeleteUserAsync(User user);
    public Task<bool> ExistUserAsync(TelegramInfo telegramInfo);
}