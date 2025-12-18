using Telegram.Bot.Types;
using Telegram.Bot;
using OzshBot.Domain.Enums;
namespace OzshBot.Bot;

public interface IBotCommand
{
    public string Name {get;}
    public bool IsAvailable(Role role);
    public string Description {get;}
    public Task<bool> ExecuteAsync(BotHandler botHandler,
                                    Update update);
}

public interface IBotCommandWithState : IBotCommand
{
    public Task TryCancelState(ITelegramBotClient bot, Chat chat,long userId);
}