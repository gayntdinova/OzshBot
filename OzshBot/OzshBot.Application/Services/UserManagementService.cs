using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.DtoModels;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Application.ToolsInterfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class UserManagementService: IUserManagementService 
{
    private readonly IUserRepository userRepository;
    private readonly ISessionRepository sessionRepository;
    private readonly ITableParser tableParser;
    public UserManagementService(IUserRepository userRepository, ISessionRepository sessionRepository, ITableParser tableParser)
    {
        this.userRepository = userRepository;
        this.sessionRepository = sessionRepository;
        this.tableParser = tableParser;
    }

    public async Task<Result<User>> AddUserAsync<T>(T user) where T: UserDtoModel
    {
        if (await userRepository.GetUserByPhoneNumberAsync(user.PhoneNumber) != null) 
            return Result.Fail(new UserAlreadyExistsError());
        var newUser = user.ToUser();
        await userRepository.AddUserAsync(newUser);
        return Result.Ok(newUser);
    }

    public async Task<Result<User>> EditUserAsync(User editedUser)
    {
        var user = await userRepository.GetUserByIdAsync(editedUser.Id);
        if (user == null)
            return Result.Fail(new UserNotFoundError());
        user.UpdateBy(editedUser);
        await userRepository.UpdateUserAsync(user);
        return Result.Ok(user);
    }

    public async Task<Result> DeleteUserAsync(string phoneNumber)
    {
        if (await userRepository.GetUserByPhoneNumberAsync(phoneNumber) == null)
            return Result.Fail(new UserNotFoundError());
        await userRepository.DeleteUserAsync(phoneNumber);
        return Result.Ok();
    }

    public async Task<Result> LoadTableAsync(string link, SessionDates sessionDates)
    {
        var session = await sessionRepository.GetSessionByDatesAsync(sessionDates);
        if (session == null) return Result.Fail(new SessionNotFoundError());
        var result = await tableParser.GetChildrenAsync(link);
        if (result.IsFailed)
        {
            if (result.HasError<IncorrectRowError>()
                || result.HasError<IncorrectUrlError>()
                || result.HasError<IncorrectTableFormatError>()) return Result.Fail(result.Errors);
            return result.ToResult();
        }
        
        foreach (var child in result.Value)
        {
            child.ChildInfo.Sessions.Add(session);
            var existedUser = await userRepository.GetUserByPhoneNumberAsync(child.PhoneNumber);
            var newUser = child.ToUser();
            if (existedUser != null)
            {
                existedUser.UpdateBy(newUser);
                await userRepository.UpdateUserAsync(existedUser);
            }
            else
            {
                await userRepository.AddUserAsync(newUser);
            }
        }
        
        return Result.Ok();
    }
}