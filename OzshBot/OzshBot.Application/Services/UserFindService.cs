using FluentResults;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class UserFindService: IUserFindService
{
    private readonly IUserRepository userRepository;
    public UserFindService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<Result<User[]>> FindUsersByClassAsync(int classNumber)
    {
        var users = await userRepository.FindUsersByClassAsync(classNumber);
        return users == null
            ? Result.Fail($"users with {classNumber} was not found")
            : Result.Ok(users);
    }

    public async Task<Result<User[]>> FindUsersByGroupAsync(int group)
    {
        var users = await userRepository.FindUsersByGroupAsync(group);
        return users == null
            ? Result.Fail($"users with {group} was not found")
            : Result.Ok(users);
    }

    public Task<Result<User[]>> FindUserAsync(string target)
    {
        throw new NotImplementedException();
    }
    
    private async Task<Result<User>> FindUserByTgAsync(TelegramInfo telegramInfo)
    {
        var user = await userRepository.FindUserByTgAsync(telegramInfo);
        return user == null 
            ? Result.Fail($"user with {telegramInfo.TgUsername} was not found") 
            : Result.Ok(user);
    }

    private async Task<Result<User[]>> FindUsersByFullNameAsync(FullName fullName)
    {
        var users = await userRepository.FindUsersByFullNameAsync(fullName);
        return users == null
            ? Result.Fail($"users with {} was not found")//todo check what exactly was completed in the fullname
            : Result.Ok(users);
    }

    private async Task<Result<User[]>> FindUsersByTownAsync(string town)
    {
        var users = await userRepository.FindUsersByTownAsync(town);
        return users == null
            ? Result.Fail($"users with {town} was not found")
            : Result.Ok(users);
    }
}