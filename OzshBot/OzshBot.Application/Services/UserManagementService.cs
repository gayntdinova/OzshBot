using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.DtoModels;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services;

public class UserManagementService: IUserManagementService 
{
    private readonly IUserRepository userRepository;
    private readonly ITableParser tableParser;
    public UserManagementService(IUserRepository userRepository, ITableParser tableParser)
    {
        this.userRepository = userRepository;
        this.tableParser = tableParser;
    }

    public async Task<Result<User>> AddUserAsync<T>(T user) where T: UserDtoModel
    {
        if (await userRepository.GetUserByTgAsync(user.TelegramInfo) != null) 
            return Result.Fail(new UserAlreadyExistsError());
        await userRepository.AddUserAsync(user.ToUser());
        return Result.Ok(user.ToUser());
    }

    public async Task<Result<User>> EditUser(User user)
    {
        await userRepository.UpdateUserAsync(user);
        return Result.Ok(user);
    }

    public async Task<Result> DeleteUserAsync(string phoneNumber)
    {
        if (await userRepository.GetUsersByPhoneNumberAsync(phoneNumber) == null)
            return Result.Fail(new NotFoundError());
        await userRepository.DeleteUserAsync(phoneNumber);
        return Result.Ok();
    }

    public async Task<Result> LoadTable(string link)
    {
        var result = await tableParser.GetChildrenAsync(link);
        if (result.IsSuccess)
        {
            foreach (var child in result.Value)
            {
                var existUser = await userRepository.GetUsersByPhoneNumberAsync(child.PhoneNumber);
                if (existUser == null)
                    await userRepository.AddUserAsync(child.ToUser());
                else
                {
                    existUser.UpdateBy(child.ToUser());
                    await userRepository.UpdateUserAsync(existUser);
                }
            }
        }
        if (result.HasError<IncorrectUrlError>()) return Result.Fail(new IncorrectUrlError());
        if (result.HasError<IncorrectRowError>()) return Result.Fail(result.Errors);
        return Result.Ok();
    }
}