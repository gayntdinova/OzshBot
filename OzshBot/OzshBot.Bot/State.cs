using Telegram.Bot.Types;
using OzshBot.Domain.ValueObjects;
using UserDomain = OzshBot.Domain.Entities.User;
namespace OzshBot.Bot;

public class State
{
    public UserState StateName;
    public UserDomain Data;
    public Stack<MessageId> messagesIds = new();

    public State(UserState name, UserDomain? user = null)
    {
        StateName = name;
        if (user == null)
            Data = new UserDomain{FullName = new(),TelegramInfo = new TelegramInfo{TgUsername = ""}};
        else
            Data = user;
    }
}