using Telegram.Bot.Types.ReplyMarkups;
using UserDomain = OzshBot.Domain.Entities.User;

namespace OzshBot.Bot.Extra;

public class UserAttributeInfo
{
    public readonly string Name;
    public readonly string WritingInfo;
    public readonly Func<string, Task<bool>> CorrectFormateFunction;
    public readonly Action<UserDomain, string> FillingAction;
    public readonly Func<UserDomain, Task<ReplyKeyboardMarkup>>? KeyboardMarkup;

    public UserAttributeInfo(string name, string writingInfo, Func<string, Task<bool>> correctFormateFunction,
        Action<UserDomain, string> fillingAction, Func<UserDomain, Task<ReplyKeyboardMarkup>>? keyboardMarkup = null)
    {
        Name = name;
        WritingInfo = writingInfo;
        CorrectFormateFunction = correctFormateFunction;
        FillingAction = fillingAction;
        KeyboardMarkup = keyboardMarkup;
    }
}