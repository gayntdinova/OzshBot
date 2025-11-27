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
        var users = await userRepository.GetUsersByClassAsync(classNumber);
        return users == null
            ? Result.Fail($"users with {classNumber} was not found")
            : Result.Ok(users);
    }

    public async Task<Result<User[]>> FindUsersByGroupAsync(int group)
    {
        var users = await userRepository.GetUsersByGroupAsync(group);
        return users == null
            ? Result.Fail($"users with {group} was not found")
            : Result.Ok(users);
    }

    public async Task<Result<User[]>> FindUserAsync(string target)
    {
        var splitedTarget = target.Split(" ");
        if (splitedTarget.Length == 1)
        {
            target = target.Replace("@", "");
            var userByTg = await FindUserByTgAsync(new TelegramInfo() { TgId = null, TgUsername = target });
            if (userByTg.IsSuccess) return Result.Ok(new []{userByTg.Value});
        }

        var usersByTown = await FindUsersByTownAsync(target);
        if (usersByTown.IsSuccess) return Result.Ok(usersByTown.Value);
        var combinations = MakeCombinationsFIOBystring(splitedTarget);
        foreach (var combination in combinations)
        {
            var userByFullName = await FindUsersByFullNameAsync(null);
            if (userByFullName.IsSuccess) return Result.Ok(userByFullName.Value);
        }
        return Result.Fail($"users with {target} was not found");
    }

    private List<FullName> MakeCombinationsFIOBystring(string[] splitedTarget)
    {
        var fullNameCombinations = new List<FullName>();
        if (splitedTarget.Length == 1)
        {
            fullNameCombinations.Add(new FullName(name: splitedTarget[0]));
            fullNameCombinations.Add(new FullName(surname: splitedTarget[0]));
            fullNameCombinations.Add(new FullName(patronymic: splitedTarget[0]));
        }

        if (splitedTarget.Length == 2)
        {
            fullNameCombinations.Add(new FullName(name: splitedTarget[0], surname: splitedTarget[1]));
            fullNameCombinations.Add(new FullName(name: splitedTarget[1], surname: splitedTarget[0]));
            fullNameCombinations.Add(new FullName(name: splitedTarget[0], patronymic: splitedTarget[1]));
        }

        if (splitedTarget.Length == 3)
        {
            fullNameCombinations.Add(new FullName(name: splitedTarget[1], surname: splitedTarget[0], patronymic: splitedTarget[2]));
        }

        return fullNameCombinations;
    }
    
    private async Task<Result<User>> FindUserByTgAsync(TelegramInfo telegramInfo)
    {
        var user = await userRepository.GetUserByTgAsync(telegramInfo);
        return user == null 
            ? Result.Fail($"user with {telegramInfo.TgUsername} was not found") 
            : Result.Ok(user);
    }

    private async Task<Result<User[]>> FindUsersByFullNameAsync(FullName fullName)
    {
        var users = await userRepository.GetUsersByFullNameAsync(fullName);
        return users == null
            ? Result.Fail($"users with {fullName.ToString()} was not found")
            : Result.Ok(users);
    }

    private async Task<Result<User[]>> FindUsersByTownAsync(string town)
    {
        var users = await userRepository.GetUsersByTownAsync(town);
        return users == null
            ? Result.Fail($"users with {town} was not found")
            : Result.Ok(users);
    }
}