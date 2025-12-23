using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;
using UserDomain = OzshBot.Domain.Entities.User;

namespace OzshBot.Bot.Extra;

public interface IFormatter
{
    public string FormatFullName(FullName fullName);

    public string FormatUser(UserDomain user, Role role);

    public string FormatUsers(IEnumerable<UserDomain> users, string message);

    public string FormatSessions(Session[] sessions, UserDomain user);

    public string FormatSessions(Session[] sessions);

    public string FormatString(string text);
}