using OzshBot.Application.DtoModels;
using OzshBot.Application.Interfaces;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application;

public class FindService: IFindService
{
    public Task<UserDto> FindUserByTgUserNameAsync(string tgName)
    {
        throw new NotImplementedException();
    }

    public Task<UsersDto> FindUsersByFullNameAsync(FullName fullName)
    {
        throw new NotImplementedException();
    }

    public Task<UsersDto> FindUsersByTownAsync(string town)
    {
        throw new NotImplementedException();
    }

    public Task<UsersDto> FindUsersByClassAsync(int classNumber)
    {
        throw new NotImplementedException();
    }

    public Task<UsersDto> FindUsersByGroupAsync(int group)
    {
        throw new NotImplementedException();
    }
}