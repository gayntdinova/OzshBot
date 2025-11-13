using OzshBot.Application.DtoModels;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Interfaces;

public interface IFindService
{
    public Task<UserDto> FindUserByTgUserNameAsync(string tgName);
    public Task<UsersDto> FindUsersByFullNameAsync(FullName fullName);
    public Task<UsersDto> FindUsersByTownAsync(string town);
    public Task<UsersDto> FindUsersByClassAsync(int classNumber);
    public Task<UsersDto> FindUsersByGroupAsync(int group);
}