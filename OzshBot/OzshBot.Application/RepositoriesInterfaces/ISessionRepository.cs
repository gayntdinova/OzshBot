using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface ISessionRepository
{
    Task AddSessionAsync(Session session);
    Task UpdateSessionAsync(Session session);
    Task<Session?> GetSessionByDatesAsync(SessionDates sessionDates);
    Task<Session?> GetSessionById(Guid sessionId);
    
    Task<Session[]?> GetLastSessionsAsync(int numberOfSessions);
    
    
}