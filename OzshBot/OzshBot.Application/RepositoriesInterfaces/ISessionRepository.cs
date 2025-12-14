using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface ISessionRepository
{
    Task AddSessionAsync(Session session);
    Task UpdateSessionAsync(Session session);
    Task<Session?> GetSessionByDatesAsync(SessionDates sessionDates);
    Task<Session?> GetSessionByIdAsync(Guid sessionId);
    Task<Session[]?> GetLastSessionsAsync(int numberOfSessions);
    Task<Session[]?> GetAllSessions();
}