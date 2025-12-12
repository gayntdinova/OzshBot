using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;

namespace OzshBot.Application;

public class SessionManager
{
    private readonly ISessionRepository sessionRepository;
    private readonly IUserRepository userRepository;

    public SessionManager(ISessionRepository sessionRepository, IUserRepository userRepository)
    {
        this.sessionRepository = sessionRepository;
        this.userRepository = userRepository;
    }

    public async Task<Session> GetOrCreateSession()
    {
        var season = GetCurrentSeason();
        var year = DateTime.Now.Year;
        var session = await sessionRepository.GetSessionBySeasonAndYearAsync(season, year);
        if (session == null)
        {
            session = new Session
            {
                Year = year,
                Season = season,
            };
            await DeleteLastSessionGroup();
            await sessionRepository.AddSessionAsync(session);
        }
        return session;
    }

    private async Task DeleteLastSessionGroup()
    {
        var lastSession = await sessionRepository.GetLastSessionAsync();
        var sessionParticipants = await userRepository.GetUsersBySessionIdAsync(lastSession.Id);
        foreach (var participant in sessionParticipants)
        {
            if (participant.Role == Role.Child)
                participant.ChildInfo!.Group = null;
            else
                participant.CounsellorInfo!.Group = null;
        }
    }

    private Season GetCurrentSeason()
    {
        var now = DateTime.UtcNow;
        return now.Month switch
        {
            1 or 2 or 12 => Season.Winter,
            3 or 4 or 5 => Season.Spring,
            6 or 7 or 8 => Season.Summer,
            9 or 10 or 11 => Season.Autumn
        };
    }
}