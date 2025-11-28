using FluentResults;
using OzshBot.Application.DtoModels;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class UserManagementService: IUserManagementService 
{
    private readonly IUserRepository userRepository;
    private readonly ITableParser tableParser;
    public UserManagementService(IUserRepository userRepository, ITableParser tableParser)
    {
        this.userRepository = userRepository;
        this.tableParser = tableParser;
    }

    public async Task<Result<User>> AddUser<T>(T user) where T: BotUserDto
    {
        if (await userRepository.GetUserByTgAsync(user.TelegramInfo) != null) return Result.Fail("User has already been added");
        await userRepository.AddUserAsync(user.ToUser());
        return Result.Ok(user.ToUser());
    }

    public async Task<Result<User>> EditUser(TelegramInfo telegramInfo, User user)
    {
        if (await userRepository.GetUserByTgAsync(telegramInfo) == null) return Result.Fail("User not found");
        await userRepository.UpdateUserAsync(user);
        return Result.Ok(user);
    }

    public async Task<Result> DeleteUserAsync(TelegramInfo telegramInfo)
    {
        if (await userRepository.GetUserByTgAsync(telegramInfo) == null) return Result.Fail("User not found");
        await userRepository.DeleteUserAsync(telegramInfo);
        return Result.Ok();
    }

    public async Task<Result> LoadTable(string link)
    {
        var children = await tableParser.GetChildrenAsync(link);
        if (children.IsSuccess)
        {
            foreach (var child in children.Value)
            {
                var existUser = await userRepository.GetUserByTgAsync(child.TelegramInfo);
                if (existUser == null)
                    await userRepository.AddUserAsync(child.ToUser());
                else
                {
                    existUser.UpdateBy(child.ToUser());
                    await userRepository.UpdateUserAsync(existUser);
                }
            }
        }
        else return Result.Fail("Error loading and parsing table");
        return Result.Ok();
        // надо добавить про смены
    }
}