
using OzshBot.Application.Services.Interfaces;

using IBLogger = OzshBot.Application.ToolsInterfaces.ILogger;
namespace OzshBot.Bot;

public class ServiseManager
{
    public IUserManagementService ManagementService;
    public IUserRoleService RoleService;
    public IUserFindService FindService;
    public ISessionService SessionService;
    public IBLogger Logger;

    public ServiseManager(
        IUserManagementService managementService,
        IUserRoleService roleService,
        IUserFindService findService,
        ISessionService sessionService,
        IBLogger logger)
    {
        ManagementService = managementService;
        RoleService = roleService;
        FindService = findService;
        SessionService = sessionService;
        Logger = logger;
    }
}