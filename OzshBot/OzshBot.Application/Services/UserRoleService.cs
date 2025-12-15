using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class UserRoleService: IUserRoleService
{
    private readonly IUserRepository userRepository;
    public UserRoleService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    public async Task<Role> GetUserRoleByTgAsync(TelegramInfo telegramInfo)
    {
        var user = await userRepository.GetUserByTgAsync(telegramInfo);
        if (user != null) UpdateTelegramInfo(user, telegramInfo);
        return user?.Role ?? Role.Unknown;
    }

    public async Task<Role> ActivateUserByPhoneNumberAsync(string phoneNumber, TelegramInfo telegramInfo)
    {
        var user = await userRepository.GetUserByPhoneNumberAsync(phoneNumber);
        if (user != null) UpdateTelegramInfo(user, telegramInfo);
        return user?.Role ?? Role.Unknown; 
    }

    public async Task<Result<User>> PromoteToCounsellorAsync(string phoneNumber)
    {
        var user = await userRepository.GetUserByPhoneNumberAsync(phoneNumber);
        if (user == null) return Result.Fail(new UserNotFoundError());
        var counsellorInfo = new CounsellorInfo
        {
            Group = null,
            Sessions = []
        };
        user.CounsellorInfo = counsellorInfo;
        user.Role = Role.Counsellor;
        await userRepository.UpdateUserAsync(user);
        return Result.Ok(user);
    }

    private void UpdateTelegramInfo(User user, TelegramInfo telegramInfo)
    {
        user.TelegramInfo = telegramInfo;
        userRepository.UpdateUserAsync(user);
    }
}