using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

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
        var existedSession = await sessionRepository.GetSessionByDatesAsync(session.SessionDates);
        if (existedSession != null) return Result.Fail(new SessionAlreadyExistsError());
        if (await CheckIfSessionIntersectsAsync(session))
            return Result.Fail(new SessionIntersectError());
        await sessionRepository.AddSessionAsync(session);
        return Result.Ok();
    }

    public async Task<Result<Session>> EditSessionAsync(Session session)
    {
        var existedSession = await sessionRepository.GetSessionByIdAsync(session.Id);
        if (existedSession == null) return Result.Fail(new SessionNotFoundError());
        if (await CheckIfSessionIntersectsAsync(session))
            return Result.Fail(new SessionIntersectError());
        await sessionRepository.UpdateSessionAsync(session);
        return Result.Ok(session);
    }  

    public async Task<Session[]> GetAllSessionsAsync()
    {
        var sessions = await sessionRepository.GetAllSessions();
        return sessions ?? [];
    }

    private async Task<bool> CheckIfSessionIntersectsAsync(Session currentSession)
    {
        var sessions = await sessionRepository.GetAllSessions();
        if (sessions == null) return false;
        foreach (var session in sessions)
        {
            if (session.Id == currentSession.Id) continue;
            if (!(session.SessionDates.StartDate >= currentSession.SessionDates.EndDate ||
                     session.SessionDates.EndDate <= currentSession.SessionDates.StartDate)) return true;
        }

        return false;
    }
}