using FluentResults;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class FindService: IFindService
{
    private readonly IUserRepository userRepository;
    public FindService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    public async Task<Result<User>> FindUserByTgAsync(TelegramInfo telegramInfo)
    {
        var user = await userRepository.FindUserByTgAsync(telegramInfo);
        return user == null 
            ? Result.Fail($"user with {telegramInfo.TgUsername} was not found") 
            : Result.Ok(user);
    }

    public async Task<User[]> FindUsersByFullNameAsync(FullName fullName)
    {
        var users = await userRepository.FindUsersByFullNameAsync(fullName);
        return users;
    }

    public async Task<User[]> FindUsersByTownAsync(string town)
    {
        var users = await userRepository.FindUsersByTownAsync(town);
        return users;
    }

    public async Task<User[]> FindUsersByClassAsync(int classNumber)
    {
        var users = await userRepository.FindUsersByClassAsync(classNumber);
        return users;
    }

    public async Task<User[]> FindUsersByGroupAsync(int group)
    {
        var users = await userRepository.FindUsersByGroupAsync(group);
        return users;
    }

    public Task<Result<User[]>> FindUserAsync(string target)
    {
        throw new NotImplementedException();
    }
}