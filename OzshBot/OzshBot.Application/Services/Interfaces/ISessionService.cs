using FluentResults;
using OzshBot.Domain.Entities;

namespace OzshBot.Application.Services.Interfaces;

public interface ISessionService
{
    Task<Result> AddSessionAsync(Session session);
    Task<Result<Session>> EditSessionAsync(Session session);
    Task<Session[]> GetAllSessionsAsync();
}