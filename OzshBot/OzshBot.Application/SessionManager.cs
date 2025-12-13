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
        if (session != null) return session;
        session = new Session
        {
            Year = year,
            Season = season,
        };
        await DeleteLastSessionGroup();
        await sessionRepository.AddSessionAsync(session);
        return session;
    }
    
    public async Task UpdateSessionsAfterAdding(User user)
    {
        switch (user.Role)
        {
            case Role.Child when user.ChildInfo!.Group != null:
            {
                var session = await GetOrCreateSession();
                user.ChildInfo.Sessions.Add(session);
                break;
            }
            case Role.Counsellor when user.CounsellorInfo!.Group != null:
            {
                var session = await GetOrCreateSession();
                user.CounsellorInfo.Sessions.Add(session);
                break;
            }
        }
    }

    public async Task UpdateSessionsAfterEditing(User oldUser, User user)
    {
        if (user.Role == Role.Child)
        {
            if (oldUser.ChildInfo!.Group == null && user.ChildInfo!.Group != null)
            {
                var session = await GetOrCreateSession();
                user.ChildInfo.Sessions.Add(session);
            }
            else if (oldUser.ChildInfo!.Group != null && user.ChildInfo!.Group == null)
            {
                user.ChildInfo.Sessions.Remove(user.ChildInfo.Sessions.Last());
            }
        }
        else if (user.Role == Role.Counsellor)
        {
            if (oldUser.CounsellorInfo!.Group == null && user.CounsellorInfo!.Group != null)
            {
                var session = await GetOrCreateSession();
                user.CounsellorInfo.Sessions.Add(session);
            }
            else if (oldUser.CounsellorInfo!.Group != null && user.CounsellorInfo!.Group == null)
            {
                user.CounsellorInfo.Sessions.Remove(user.CounsellorInfo.Sessions.Last());
            }
        }
    }

    private async Task DeleteLastSessionGroup()
    {
        var lastSession = await sessionRepository.GetLastSessionAsync();
        if (lastSession != null)
        {
            var sessionParticipants = await userRepository.GetUsersBySessionIdAsync(lastSession.Id);
            foreach (var participant in sessionParticipants)
            {
                if (participant.Role == Role.Child)
                    participant.ChildInfo!.Group = null;
                else
                    participant.CounsellorInfo!.Group = null;
            }
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