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

    public async Task<Role> ActivateUserByPhoneNumber(string phoneNumber, TelegramInfo telegramInfo)
    {
        var user = await userRepository.GetUsersByPhoneNumberAsync(phoneNumber);
        if (user != null) UpdateTelegramInfo(user, telegramInfo);
        return user?.Role ?? Role.Unknown; 
    }

    public async Task<Result<User>> PromoteToCounsellor(TelegramInfo telegramInfo)
    {
        var user = await userRepository.GetUserByTgAsync(telegramInfo);
        if (user == null) return Result.Fail(new NotFoundError());
        var counsellorInfo = new CounsellorInfo
        {
            Group = null,
            Sessions = []
        };
        user.CounsellorInfo = counsellorInfo;
        await userRepository.UpdateUserAsync(user);
        return Result.Ok(user);
    }

    private void UpdateTelegramInfo(User user, TelegramInfo telegramInfo)
    {
        user.TelegramInfo = telegramInfo;
        userRepository.UpdateUserAsync(user);
    }
}