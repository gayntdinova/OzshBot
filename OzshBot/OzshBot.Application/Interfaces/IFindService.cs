using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Interfaces;

public interface IFindService
{
    public Task<UserDto> FindUserByTgUserNameAsync(string tgName);
    public Task<ManyUsersDto> FindUsersByFullNameAsync(FullName fullName);
    public Task<ManyUsersDto> FindUsersByTownAsync(string town);
    public Task<ManyUsersDto> FindUsersByClassAsync(int classNumber);
    public Task<ManyUsersDto> FindUsersByGroupAsync(int group);
}