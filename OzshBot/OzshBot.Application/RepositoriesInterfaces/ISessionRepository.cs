using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;

namespace OzshBot.Application.RepositoriesInterfaces;

public interface ISessionRepository
{
    Task AddSessionAsync(Session session);
    Task<Session?> GetSessionBySeasonAndYearAsync(Season season, int year);
    Task<Session> GetLastSessionAsync();
}