using OzshBot.Application.Services.Interfaces;
using IBLogger = OzshBot.Application.ToolsInterfaces.ILogger;

namespace OzshBot.Bot.Extra;

public class ServiceManager
{
    public readonly IUserManagementService ManagementService;
    public readonly IUserRoleService RoleService;
    public readonly IUserFindService FindService;
    public readonly ISessionService SessionService;
    public readonly IBLogger Logger;

    public ServiceManager(
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