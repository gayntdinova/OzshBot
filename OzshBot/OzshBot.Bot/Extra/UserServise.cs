using OzshBot.Application.Services.Interfaces;
namespace OzshBot.Bot;

public class UserService
{
    public IUserManagementService ManagementService;
    public IUserRoleService RoleService;
    public IUserFindService FindService;
    public ISessionService SessionService;

    public UserService(
        IUserManagementService managementService,
        IUserRoleService roleService,
        IUserFindService findService,
        ISessionService sessionService)
    {
        ManagementService = managementService;
        RoleService = roleService;
        FindService = findService;
        SessionService = sessionService;
    }
}