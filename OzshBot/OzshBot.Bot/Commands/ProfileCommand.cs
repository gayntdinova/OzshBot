using OzshBot.Bot.Extra;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OzshBot.Bot.Commands;

public class ProfileCommand : IBotCommand
{
    private readonly Role[] roles = [Role.Child, Role.Counsellor];

    public string Name
        => "/profile";

    public bool IsAvailable(Role role)
    {
        return roles.Contains(role);
    }

    public string Description
        => "Мой профиль";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
        Update update)
    {
        var bot = botHandler.BotClient;
        var serviceManager = botHandler.ServiceManager;
        var formatter = botHandler.Formatter;

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
            formatter.FormatUser(you,you.Role),
            replyMarkup: new ReplyKeyboardRemove(),
            parseMode: ParseMode.MarkdownV2);

        return false;
    }
}