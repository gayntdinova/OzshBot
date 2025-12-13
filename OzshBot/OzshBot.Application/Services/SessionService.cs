using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;

namespace OzshBot.Application.Services;

public class SessionService: ISessionService
{
    private readonly ISessionRepository sessionRepository;
    
    public SessionService(ISessionRepository sessionRepository)
    {
        this.sessionRepository = sessionRepository;
    }
    
    public async Task<Result> AddSessionAsync(Session session)
    {
        var existedSession = await sessionRepository.GetSessionById(session.Id);
        if (existedSession != null) return Result.Fail(new SessionAlreadyExistsError());
        await sessionRepository.AddSessionAsync(session);
        return Result.Ok();
    }

    public async Task<Result<Session>> EditSessionAsync(Session session)
    {
        var existedSession = await sessionRepository.GetSessionById(session.Id);
        if (existedSession == null) return Result.Fail(new SessionNotFoundError());
        await sessionRepository.UpdateSessionAsync(session);
        return Result.Ok(session);
    }

    public async Task<Result<Session[]>> GetLastSessionsAsync(int numberOfSessions)
    {
        var sessions = await sessionRepository.GetLastSessionsAsync(numberOfSessions);
        if (sessions == null) return Result.Fail(new SessionNotFoundError());
        return Result.Ok(sessions);
    }
}