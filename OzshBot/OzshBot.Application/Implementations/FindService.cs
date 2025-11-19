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

    public async Task<ManyUsersDto> FindUsersByFullNameAsync(FullName fullName)
    {
        throw new NotImplementedException();
    }

    public async Task<ManyUsersDto> FindUsersByTownAsync(string town)
    {
        throw new NotImplementedException();
    }

    public async Task<ManyUsersDto> FindUsersByClassAsync(int classNumber)
    {
        throw new NotImplementedException();
    }

    public async Task<ManyUsersDto> FindUsersByGroupAsync(int group)
    {
        throw new NotImplementedException();
    }
}