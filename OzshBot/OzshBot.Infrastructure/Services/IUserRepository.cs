using OzshBot.Infrastructure.DTO;
using OzshBot.Infrastructure.Enums;

namespace OzshBot.Infrastructure.Services;

public interface IUserRepository
{
    public DbUser GetUser(string tgName);
    public void AddUser(DbUser user);
    public void ChangeUser(DbUser newUserData);
    public void DeleteUser(Guid userId);

    public Access GetAccessRights(string tgname);
    public void ChangeAccessRights(string tgName, Access newRights);
}