using OzshBot.Application.Services.Interfaces;
namespace OzshBot.Bot;

public class UserService
{
    public IUserManagementService ManagementService;
    public IUserRoleService RoleService;
    public IUserFindService FindService;

    public UserService(
        IUserManagementService managementService,
        IUserRoleService roleService,
        IUserFindService findService)
    {
        ManagementService = managementService;
        RoleService = roleService;
        FindService = findService;
    }
}