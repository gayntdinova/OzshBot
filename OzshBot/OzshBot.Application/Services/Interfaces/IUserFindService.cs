using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserFindService
{
    Task<User[]> FindUsersByClassAsync(int classNumber);
    Task<User[]> FindUsersByGroupAsync(int group);
    Task<User?> FindUserByTgAsync(TelegramInfo telegramInfo);
    Task<User?> FindUserByPhoneNumberAsync(string phoneNumber);
    Task<User[]> FindUserAsync(string input);
    
} 