using FluentResults;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserFindService
{
    public Task<User[]> FindUsersByClassAsync(int classNumber);
    public Task<User[]> FindUsersByGroupAsync(int group);
    public Task<User?> FindUserByTgAsync(TelegramInfo telegramInfo);
    public Task<User?> FindUserByPhoneNumberAsync(string phoneNumber);
    public Task<User[]> FindUserAsync(string input);
    
} 