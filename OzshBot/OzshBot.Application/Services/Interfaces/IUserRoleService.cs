using FluentResults;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface IUserRoleService
{
    Task<Role> GetUserRoleByTgAsync(TelegramInfo telegramInfo);
    Task<Role> ActivateUserByPhoneNumberAsync(string phoneNumber, TelegramInfo telegramInfo);
    
    Task<Result<User>> PromoteToCounsellorAsync(string phoneNumber);
 }