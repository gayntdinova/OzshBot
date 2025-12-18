using Microsoft.EntityFrameworkCore;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;
using OzshBot.Infrastructure.Data;
using OzshBot.Infrastructure.Models;

namespace OzshBot.Infrastructure.Services;

public class SessionsRepository(AppDbContext context) : ISessionRepository
{
    private readonly AppDbContext context = context;

    public async Task AddSessionAsync(Domain.Entities.Session session)
    {
        var existingSession = await GetSessionByIdAsync(session.Id);
        if (existingSession != null) throw new InvalidOperationException($"Уже есть смена с таким session_id: {session.Id}");
        if (session.SessionDates.StartDate >= session.SessionDates.EndDate)
        {
            throw new ArgumentException("Start date must be before end date");
        }
        await context.Sessions.AddAsync(SessionConverter.FromDomainSession(session));
        await context.SaveChangesAsync();
    }

    public async Task<Domain.Entities.Session[]?> GetAllSessions()
    {
        return await context.Sessions
            .OrderBy(s => s.StartDate)
            .Select(s => s.ToDomainSession())
            .ToArrayAsync();
    }

    public async Task<Domain.Entities.Session[]?> GetLastSessionsAsync(int numberOfSessions)
    {
        return await context.Sessions
            .OrderBy(s => s.StartDate)
            .Take(numberOfSessions)
            .Select(s => s.ToDomainSession())
            .ToArrayAsync();
    }
    public async Task<Domain.Entities.Session?> GetLastSessionAsync()
    {
        return await context.Sessions
            .OrderBy(s => s.StartDate)
            .Select(s => s.ToDomainSession())
            .FirstOrDefaultAsync();
    }

    public async Task<Domain.Entities.Session?> GetSessionByDatesAsync(SessionDates sessionDates)
    {
        return await context.Sessions
            .Where(s => s.StartDate == sessionDates.StartDate && s.EndDate == sessionDates.EndDate)
            .Select(s => s.ToDomainSession())
            .FirstOrDefaultAsync();
    }

    public async Task<Domain.Entities.Session?> GetSessionByIdAsync(Guid sessionId)
    {
        return await context.Sessions
            .Where(s => s.SessionId == sessionId)
            .Select(s => s.ToDomainSession())
            .FirstOrDefaultAsync();
    }

    public async Task UpdateSessionAsync(Domain.Entities.Session session)
    {
        var existingSession = await GetSessionByIdAsync(session.Id);
        if (existingSession == null) throw new InvalidOperationException($"Нет смены с таким session_id: {session.Id}");
        var dbSession = SessionConverter.FromDomainSession(existingSession);
        dbSession.StartDate = session.SessionDates.StartDate;
        dbSession.EndDate = session.SessionDates.EndDate;
        await context.SaveChangesAsync();
    }
}