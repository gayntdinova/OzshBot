using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Telegram.Bot.Types.ReplyMarkups;
namespace OzshBot.Bot;


public class ProfileCommand : IBotCommand
{
    private readonly Role[] roles = new[]{Role.Child, Role.Counsellor};
    public string Name
    => "/profile";

    public bool IsAvailable(Role role)
    => roles.Contains(role);

    public string Description
    => "Мой профиль";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
                                        Update update)
    {
        var bot = botHandler.BotClient;
        var serviceManager = botHandler.ServiceManager;
        
        var message = update.Message!;
        var username = message.From!.Username!;
        var chat = message.Chat;

        var you = await serviceManager.FindService.FindUserByTgAsync(
            new TelegramInfo { TgUsername = username });

        if (you == null)
        {
            await bot.SendMessage(
                chat.Id,
                "вас не существует",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2);
            return false;
        }

        await bot.SendMessage(
            chat.Id, 
            you.FormateAnswer(you.Role),
            replyMarkup: new ReplyKeyboardRemove(),
            parseMode: ParseMode.MarkdownV2);

        return false;
    }
}