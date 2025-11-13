using FluentResults;

namespace OzshBot.Application.Interfaces;

public interface IEditService
{
    public Task<Result> EditUserAsync<T>(T user);
    public Task<Result> AddUserAsync<T>(T user);
    public Task<Result> DeleteUserAsync<T>(T user);
}