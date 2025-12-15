using FluentResults;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.Services.Interfaces;

public interface ISessionService
{
    Task<Result> AddSessionAsync(Session session);
    Task<Result<Session>> EditSessionAsync(Session session);
    Task<Result<Session[]>> GetLastSessionsAsync(int numberOfSessions);
}