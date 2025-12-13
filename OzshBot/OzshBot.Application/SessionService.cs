using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;

namespace OzshBot.Application;

public class SessionService
{
    private readonly ISessionRepository sessionRepository;
    private readonly IUserRepository userRepository;

    public SessionService(ISessionRepository sessionRepository, IUserRepository userRepository)
    {
        this.sessionRepository = sessionRepository;
        this.userRepository = userRepository;
    }

    public async Task<Session> GetOrCreateSessionAsync()
    {
        var now = DateTime.Now;
        var season = GetSeasonByMonth(now.Month);
        var year = now.Year;
        var session = await sessionRepository.GetSessionBySeasonAndYearAsync(season, year);
        if (session != null) return session;
        session = new Session
        {
            Year = year,
            Season = season,
        };
        await DeleteLastSessionGroupAsync();
        await sessionRepository.AddSessionAsync(session);
        return session;
    }

    private async Task DeleteLastSessionGroupAsync()
    {
        var now = DateTime.Now;
        var lastSession = await sessionRepository.GetSessionBySeasonAndYearAsync(GetSeasonByMonth(now.Month - 1), now.Year);
        if (lastSession != null)
        {
            var sessionParticipants = await userRepository.GetUsersBySessionIdAsync(lastSession.Id);
            if (sessionParticipants != null)
            {
                foreach (var participant in sessionParticipants)
                {
                   
                    if (participant.CounsellorInfo != null)
                        participant.CounsellorInfo!.Group = null;
                    if (participant.ChildInfo != null)
                        participant.ChildInfo!.Group = null;
                    await userRepository.UpdateUserAsync(participant);
                }
            }
        }
    }

    private static Season GetSeasonByMonth(int month)
    {
        return month switch
        {
            1 or 2 or 12 => Season.Winter,
            3 or 4 or 5 => Season.Spring,
            6 or 7 or 8 => Season.Summer,
            9 or 10 or 11 => Season.Autumn
        };
    }
}