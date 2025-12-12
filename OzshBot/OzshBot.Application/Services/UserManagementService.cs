using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.DtoModels;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Application.ToolsInterfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums; 

namespace OzshBot.Application.Services;

public class UserManagementService: IUserManagementService 
{
    private readonly IUserRepository userRepository;
    private readonly ITableParser tableParser;
    private readonly SessionManager sessionManager;
    public UserManagementService(IUserRepository userRepository, ISessionRepository sessionRepository, ITableParser tableParser)
    {
        this.userRepository = userRepository;
        this.tableParser = tableParser;
        sessionManager = new SessionManager(sessionRepository, userRepository);
    }

    public async Task<Result<User>> AddUserAsync<T>(T user) where T: UserDtoModel
    {
        if (await userRepository.GetUserByPhoneNumberAsync(user.PhoneNumber) != null) 
            return Result.Fail(new UserAlreadyExistsError());
        var newUser = user.ToUser();
        await UpdateSessionsAfterAdding(newUser);
        await userRepository.AddUserAsync(newUser);
        return Result.Ok(user.ToUser());
    }

    public async Task<Result<User>> EditUserAsync(User editedUser, string phoneNumber)
    {
        var user = await userRepository.GetUserByPhoneNumberAsync(phoneNumber);
        if (user == null)
            return Result.Fail(new NotFoundError());
        await UpdateSessionsAfterEditing(user, editedUser);
        user.UpdateBy(editedUser);
        await userRepository.UpdateUserAsync(editedUser);
        return Result.Ok(user);
    }

    public async Task<Result> DeleteUserAsync(string phoneNumber)
    {
        if (await userRepository.GetUserByPhoneNumberAsync(phoneNumber) == null)
            return Result.Fail(new NotFoundError());
        await userRepository.DeleteUserAsync(phoneNumber);
        return Result.Ok();
    }

    public async Task<Result> LoadTableAsync(string link)
    {
        var result = await tableParser.GetChildrenAsync(link);
        if (result.IsSuccess)
        {
            if (result.Value.Any(child => child.ChildInfo.Group != null))
            {
                var session = await sessionManager.GetOrCreateSession();
                foreach (var child in result.Value)
                {
                    if (child.ChildInfo.Group is null) continue;
                    child.ChildInfo.Sessions.Add(session);
                    var existedUser = await userRepository.GetUserByPhoneNumberAsync(child.PhoneNumber);
                    if (existedUser != null)
                    {
                        existedUser.UpdateBy(child.ToUser());
                        await userRepository.UpdateUserAsync(existedUser);
                    }
                    else
                    {
                        await userRepository.AddUserAsync(child.ToUser());
                    }
                }
            }
        }
        if (result.HasError<IncorrectUrlError>()) return Result.Fail(new IncorrectUrlError());
        if (result.HasError<IncorrectRowError>()) return Result.Fail(result.Errors);
        return Result.Ok();
    }

    private async Task UpdateSessionsAfterAdding(User user)
    {
        switch (user.Role)
        {
            case Role.Child when user.ChildInfo!.Group != null:
            {
                var session = await sessionManager.GetOrCreateSession();
                user.ChildInfo.Sessions.Add(session);
                break;
            }
            case Role.Counsellor when user.CounsellorInfo!.Group != null:
            {
                var session = await sessionManager.GetOrCreateSession();
                user.CounsellorInfo.Sessions.Add(session);
                break;
            }
        }
    }

    private async Task UpdateSessionsAfterEditing(User editedUser, User user)
    {
        if (editedUser.Role == Role.Child)
        {
            if (editedUser.ChildInfo!.Group == null && user.ChildInfo!.Group != null)
            {
                var session = await sessionManager.GetOrCreateSession();
                editedUser.ChildInfo.Sessions.Add(session);
            }
            else if (editedUser.ChildInfo!.Group != null && user.ChildInfo!.Group == null)
            {
                editedUser.ChildInfo.Sessions.Remove(editedUser.ChildInfo.Sessions.Last());
            }
        }
        else if (editedUser.Role == Role.Counsellor)
        {
            if (editedUser.CounsellorInfo!.Group == null && user.CounsellorInfo!.Group != null)
            {
                var session = await sessionManager.GetOrCreateSession();
                editedUser.CounsellorInfo.Sessions.Add(session);
            }
            else if (editedUser.CounsellorInfo!.Group != null && user.CounsellorInfo!.Group == null)
            {
                editedUser.CounsellorInfo.Sessions.Remove(editedUser.CounsellorInfo.Sessions.Last());
            }
        }
    }
    
}