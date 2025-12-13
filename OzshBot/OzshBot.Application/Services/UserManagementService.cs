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
    private readonly SessionManager sessionManager;
    public UserManagementService(IUserRepository userRepository, SessionManager sessionManager, ITableParser tableParser)
    {
        this.userRepository = userRepository;
        this.tableParser = tableParser;
        this.sessionManager = sessionManager;
    }

    public async Task<Result<User>> AddUserAsync<T>(T user) where T: UserDtoModel
    {
        if (await userRepository.GetUserByPhoneNumberAsync(user.PhoneNumber) != null) 
            return Result.Fail(new UserAlreadyExistsError());
        var newUser = user.ToUser();
        await sessionManager.UpdateSessionsAfterAdding(newUser);
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
        await sessionManager.UpdateSessionsAfterEditing(oldUser, user);
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
                await sessionManager.UpdateSessionsAfterEditing(oldUser, existedUser);
                await userRepository.UpdateUserAsync(existedUser);
            }
            else
            {
                var newUser = child.ToUser();
                await sessionManager.UpdateSessionsAfterAdding(newUser);
                await userRepository.AddUserAsync(newUser);
            }
        }
        return Result.Ok();
    }
}