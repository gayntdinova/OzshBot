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

    public async Task<Result<User[]>> FindUserAsync(string input)
    {
        var splitedInput = input.Split(" ");
        if (splitedInput.Length == 1)
        {
            input = input.Replace("@", "");
            var userByTg = await FindUserByTgAsync(new TelegramInfo { TgId = null, TgUsername = input });
            if (userByTg.IsSuccess) return Result.Ok(new[] {userByTg.Value});
        }

        var usersByTown = await FindUsersByCityAsync(input);
        if (usersByTown.IsSuccess) return Result.Ok(usersByTown.Value);
        
        var usersBySchool = await FindUsersBySchoolAsync(input);
        if (usersBySchool.IsSuccess) return Result.Ok(usersBySchool.Value);
        
        var combinations = GenerateFullNameCombinationsByInput(splitedInput);
        foreach (var combination in combinations)
        {
            var userByFullName = await FindUsersByFullNameAsync(combination);
            if (userByFullName.IsSuccess) return Result.Ok(userByFullName.Value);
        }
        return Result.Fail($"users with {input} was not found");
    }
    
    public async Task<Result<User>> FindUserByTgAsync(TelegramInfo telegramInfo)
    {
        var user = await userRepository.GetUserByTgAsync(telegramInfo);
        return user == null 
            ? Result.Fail($"user with {telegramInfo.TgUsername} was not found") 
            : Result.Ok(user);
    }

    private static List<FullName> GenerateFullNameCombinationsByInput(string[] splitedTarget)
    {
        var fullNameCombinations = new List<FullName>();
        switch (splitedTarget.Length)
        {
            case 1:
                fullNameCombinations.Add(new FullName(name: splitedTarget[0]));
                fullNameCombinations.Add(new FullName(surname: splitedTarget[0]));
                fullNameCombinations.Add(new FullName(patronymic: splitedTarget[0]));
                break;
            case 2:
                fullNameCombinations.Add(new FullName(name: splitedTarget[0], surname: splitedTarget[1]));
                fullNameCombinations.Add(new FullName(name: splitedTarget[1], surname: splitedTarget[0]));
                fullNameCombinations.Add(new FullName(name: splitedTarget[0], patronymic: splitedTarget[1]));
                break;
            case 3:
                fullNameCombinations.Add(new FullName(name: splitedTarget[1], surname: splitedTarget[0], patronymic: splitedTarget[2]));
                break;
        }

        return fullNameCombinations;
    }

    private async Task<Result<User[]>> FindUsersByFullNameAsync(FullName fullName)
    {
        var users = await userRepository.GetUsersByFullNameAsync(fullName);
        return users == null
            ? Result.Fail($"users with {fullName.ToString()} was not found")
            : Result.Ok(users);
    }

    private async Task<Result<User[]>> FindUsersByCityAsync(string city)
    {
        var users = await userRepository.GetUsersByCityAsync(city);
        return users == null
            ? Result.Fail($"users with {city} was not found")
            : Result.Ok(users);
    }

    private async Task<Result<User[]>> FindUsersBySchoolAsync(string school)
    {
        var users = await userRepository.GetUsersBySchoolAsync(school);
        return users == null
            ? Result.Fail($"users with {school} was not found")
            : Result.Ok(users);
    }
}