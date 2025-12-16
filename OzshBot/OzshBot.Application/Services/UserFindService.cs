using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class UserFindService: IUserFindService
{
    private readonly IUserRepository userRepository;
    public UserFindService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }
    
    public async Task<User[]> FindUsersByClassAsync(int classNumber)
    {
        var users = await userRepository.GetUsersByClassAsync(classNumber);
        if (users == null) return [];
        return users.Where(user => user.Role == Role.Child && user.ChildInfo.Group != null)
            .ToArray();
    }
    
    public async Task<User[]> FindUsersByGroupAsync(int group)
    {
        var users = await userRepository.GetUsersByGroupAsync(group);
        return users ?? [];
    }

    public async Task<User?> FindUserByPhoneNumberAsync(string phoneNumber)
    {
        var user = await userRepository.GetUserByPhoneNumberAsync(phoneNumber);
        return user;
    }

    public async Task<User[]> FindUserAsync(string input)
    {
        var splitedInput = input.Split(" ");
        if (splitedInput.Length == 1)
        {
            var tg = input.Replace("@", "");
            var userByTg = await FindUserByTgAsync(new TelegramInfo { TgId = null, TgUsername = input });
            if (userByTg is not null) return [userByTg];
        }

        var usersByCity = await FindUsersByCityAsync(input);
        if (usersByCity.Length != 0) return usersByCity;
        
        var usersBySchool = await FindUsersBySchoolAsync(input);
        if (usersBySchool.Length != 0) return usersBySchool;
        
        var combinations = GenerateFullNameCombinationsByInput(splitedInput);
        foreach (var combination in combinations)
        {
            var userByFullName = await FindUsersByFullNameAsync(combination);
            if (userByFullName.Length != 0) return userByFullName;
        }
        return [];
    }
    
    public async Task<User?> FindUserByTgAsync(TelegramInfo telegramInfo)
    {
        var user = await userRepository.GetUserByTgAsync(telegramInfo);
        return user;
    }

    private async Task<User[]> FindUsersByFullNameAsync(NameSearch name)
    {
        var users = await userRepository.GetUsersByFullNameAsync(name);
        return users ?? [];
    }

    private async Task<User[]> FindUsersByCityAsync(string city)
    {
        var users = await userRepository.GetUsersByCityAsync(city);
        return users ?? [];
    }

    private async Task<User[]> FindUsersBySchoolAsync(string school)
    {
        var users = await userRepository.GetUsersBySchoolAsync(school);
        return users ?? [];
    } 
    
    private static List<NameSearch> GenerateFullNameCombinationsByInput(string[] splitedTarget)
    {
        var fullNameCombinations = new List<NameSearch>();
        switch (splitedTarget.Length)
        {
            case 1:
                fullNameCombinations.Add(new NameSearch(name: splitedTarget[0]));
                fullNameCombinations.Add(new NameSearch(surname: splitedTarget[0]));
                fullNameCombinations.Add(new NameSearch(patronymic: splitedTarget[0]));
                break;
            case 2:
                fullNameCombinations.Add(new NameSearch(name: splitedTarget[0], surname: splitedTarget[1]));
                fullNameCombinations.Add(new NameSearch(name: splitedTarget[1], surname: splitedTarget[0]));
                fullNameCombinations.Add(new NameSearch(name: splitedTarget[0], patronymic: splitedTarget[1]));
                break;
            case 3:
                fullNameCombinations.Add(new NameSearch(name: splitedTarget[1], surname: splitedTarget[0], patronymic: splitedTarget[2]));
                break;
        }

        return fullNameCombinations;
    }
}