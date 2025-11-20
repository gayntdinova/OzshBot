using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class FindService: IFindService
{
    public async Task<User> FindUserByTgAsync(TelegramInfo telegramInfo)
    {
        throw new NotImplementedException();
    }

    public async Task<User[]> FindUsersByFullNameAsync(FullName fullName)
    {
        throw new NotImplementedException();
    }

    public async Task<User[]> FindUsersByTownAsync(string town)
    {
        throw new NotImplementedException();
    }

    public async Task<User[]> FindUsersByClassAsync(int classNumber)
    {
        throw new NotImplementedException();
    }

    public async Task<User[]> FindUsersByGroupAsync(int group)
    {
        throw new NotImplementedException();
    }

    public async Task<User[]> FindUserByAsync(string target)
    {
        throw new NotImplementedException();
    }
}