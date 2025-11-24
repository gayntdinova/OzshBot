using FluentResults;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class RoleService: IRoleService
{
    private readonly IUserRepository userRepository;
    public RoleService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    public async Task<Role> GetUserRole(TelegramInfo telegramInfo)
    {
        var user = await userRepository.FindUserByTgAsync(telegramInfo);
        return user?.Role ?? Role.Unknown;
    }

    public async Task<Result<User>> PromoteToCounsellor(TelegramInfo telegramInfo)
    {
        var user = await userRepository.FindUserByTgAsync(telegramInfo);
        if (user == null) return Result.Fail("User not found");
        var counsellorInfo = new CounsellorInfo
        {
            FullName = user.ChildInfo.FullName,
            Birthday = user.ChildInfo.Birthday,
            City = user.ChildInfo.City,
            PhoneNumber = user.ChildInfo.PhoneNumber,
            Email = user.ChildInfo.Email,
            Group = user.ChildInfo.Group,
            Sessions = new Session[0]
        };
        user.CounsellorInfo = counsellorInfo;
        await userRepository.UpdateUserAsync(user);
        return Result.Ok(user);
    }
}