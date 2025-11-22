using OzshBot.Domain.Entities;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface ISessionRepository
{
    Task AddSessionAsync(Session session);
    Task UpdateSessionAsync(Session session);
    Task DeleteSessionAsync(Session session);
}