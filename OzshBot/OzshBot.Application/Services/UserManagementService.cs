using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.DtoModels;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Application.ToolsInterfaces;
using OzshBot.Domain;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums; 

namespace OzshBot.Application.Services;

public class UserManagementService: IUserManagementService 
{
    private readonly IUserRepository userRepository;
    private readonly ITableParser tableParser;
    private readonly SessionService _sessionService;
    public UserManagementService(IUserRepository userRepository, SessionService sessionService, ITableParser tableParser)
    {
        this.userRepository = userRepository;
        this.tableParser = tableParser;
        this._sessionService = sessionService;
    }

    public async Task<Result<User>> AddUserAsync<T>(T user) where T: UserDtoModel
    {
        if (await userRepository.GetUserByPhoneNumberAsync(user.PhoneNumber) != null) 
            return Result.Fail(new UserAlreadyExistsError());
        var newUser = user.ToUser();
        await UpdateSessionsAfterUserAdditionAsync(newUser);
        await userRepository.AddUserAsync(newUser);
        return Result.Ok(newUser);
    }

    public async Task<Result<User>> EditUserAsync(User editedUser, string phoneNumber)
    {
        var user = await userRepository.GetUserByPhoneNumberAsync(phoneNumber);
        if (user == null)
            return Result.Fail(new NotFoundError());
        var oldUser = user.Clone();
        user.UpdateBy(editedUser);
        await UpdateSessionsAfterUserEditingAsync(oldUser, user);
        await userRepository.UpdateUserAsync(user);
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
        if (result.IsFailed)
        {
            if (result.HasError<IncorrectUrlError>()) return Result.Fail(new IncorrectUrlError());
            if (result.HasError<IncorrectRowError>()) return Result.Fail(result.Errors);
            return result.ToResult();
        }
        

        foreach (var child in result.Value)
        {
            var existedUser = await userRepository.GetUserByPhoneNumberAsync(child.PhoneNumber);
            if (existedUser != null)
            {
                var oldUser = existedUser.Clone();
                existedUser.UpdateBy(child.ToUser());
                await UpdateSessionsAfterUserEditingAsync(oldUser, existedUser);
                await userRepository.UpdateUserAsync(existedUser);
            }
            else
            {
                var newUser = child.ToUser();
                await UpdateSessionsAfterUserAdditionAsync(newUser);
                await userRepository.AddUserAsync(newUser);
            }
        }
        return Result.Ok();
    }
    
    private async Task UpdateSessionsAfterUserAdditionAsync(User user)
    {
        switch (user.Role)
        {
            case Role.Child when user.ChildInfo!.Group != null:
            {
                var session = await _sessionService.GetOrCreateSessionAsync();
                user.ChildInfo.Sessions.Add(session);
                break;
            }
            case Role.Counsellor when user.CounsellorInfo!.Group != null:
            {
                var session = await _sessionService.GetOrCreateSessionAsync();
                user.CounsellorInfo.Sessions.Add(session);
                break;
            }
        }
    }

    private async Task UpdateSessionsAfterUserEditingAsync(User oldUser, User user)
    {
        if (user.Role == Role.Child)
        {
            if (oldUser.ChildInfo!.Group == null && user.ChildInfo!.Group != null)
            {
                var session = await _sessionService.GetOrCreateSessionAsync();
                user.ChildInfo.Sessions.Add(session);
            }
            else if (oldUser.ChildInfo!.Group != null && user.ChildInfo!.Group == null)
            {
                user.ChildInfo.Sessions.Remove(user.ChildInfo.Sessions.Last());
            }
        }
        else if (user.Role == Role.Counsellor)
        {
            if (oldUser.CounsellorInfo!.Group == null && user.CounsellorInfo!.Group != null)
            {
                var session = await _sessionService.GetOrCreateSessionAsync();
                user.CounsellorInfo.Sessions.Add(session);
            }
            else if (oldUser.CounsellorInfo!.Group != null && user.CounsellorInfo!.Group == null)
            {
                user.CounsellorInfo.Sessions.Remove(user.CounsellorInfo.Sessions.Last());
            }
        }
    }
}