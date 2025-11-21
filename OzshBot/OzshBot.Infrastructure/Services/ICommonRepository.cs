using OzshBot.Domain.Entities;
namespace OzshBot.Infrastructure.Services;

public interface ICommonRepository
{
    public User FindByTgName(string tgname);
    public List<User> FindByName(string? name, string? surname, string? patronymic);
    public List<User> FindByCity(string city);
    public List<User> FindBySchool(string school);

    public List<User> GetGroupList(int group);
    public List<User> GetClassList(int classNumber);

    public void AddChild(ChildInfo child);
    public void ChangeChild(ChildInfo newChildData);
    public void AddCounsellor(CounsellorInfo counsellor);
    public void ChangeCounsellor(CounsellorInfo newCounsellorData);

    public void DeleteUserById(Guid userId);
    public void DeleteUserByTgName(string tgname);
}