using FluentResults;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserRoleService
{
    public Task<Role> GetUserRoleByTgAsync(TelegramInfo telegramInfo);
    public Task<Role> ActivateUserByPhoneNumberAsync(string phoneNumber, TelegramInfo telegramInfo);
    
    public Task<Result<User>> PromoteToCounsellorAsync(string phoneNumber);
 }