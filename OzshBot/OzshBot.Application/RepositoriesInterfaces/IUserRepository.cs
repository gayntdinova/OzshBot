using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface IUserRepository
{
    Task<User?> GetUserByTgAsync(TelegramInfo telegramInfo);
    Task<User[]?> GetUsersByFullNameAsync(FullName fullName);
    Task<User[]?> GetUsersByCityAsync(string city);
    Task<User[]?> GetUsersByClassAsync(int classNumber);
    Task<User[]?> GetUsersByGroupAsync(int group);
    Task<User[]?> GetUsersBySchoolAsync(string school);
    Task<User?> GetUsersByPhoneNumberAsync(string phoneNumber);
    Task AddUserAsync(User user);
    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(TelegramInfo telegramInfo);
}