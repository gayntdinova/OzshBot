using OzshBot.Application.DtoModels;
using OzshBot.Application.Interfaces;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Implementations;

public class FindService: IFindService
{
    public async Task<UserDto> FindUserByTgUserNameAsync(string tgName)
    {
        throw new NotImplementedException();
    }

    public async Task<UsersDto> FindUsersByFullNameAsync(FullName fullName)
    {
        throw new NotImplementedException();
    }

    public async Task<UsersDto> FindUsersByTownAsync(string town)
    {
        throw new NotImplementedException();
    }

    public async Task<UsersDto> FindUsersByClassAsync(int classNumber)
    {
        throw new NotImplementedException();
    }

    public async Task<UsersDto> FindUsersByGroupAsync(int group)
    {
        throw new NotImplementedException();
    }
}