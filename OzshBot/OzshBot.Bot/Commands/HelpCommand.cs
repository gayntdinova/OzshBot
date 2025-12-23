using OzshBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OzshBot.Bot.Commands;

public class HelpCommand : IBotCommand
{
    private readonly Role[] roles = [Role.Child, Role.Counsellor];

    public string Name
        => "/help";

    public bool IsAvailable(Role role)
    {
        return roles.Contains(role);
    }

    public string Description
        => "Помощь";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
        Update update)
    {
        var bot = botHandler.BotClient;

        var message = update.Message!;
        var chat = message.Chat;
        var formatter = botHandler.Formatter;

        await bot.SendMessage(
            chat.Id,
            formatter.FormatString(
            "🤖 *Помощь по боту*\n\n" +
            "Этот бот предназначен для *поиска информации о пользователях*\\.\n\n" +
            "🔍 *Поиск пользователей*\n" +
            "Вы можете искать пользователей по следующим параметрам:\n" +
            "\\- имени, фамилии или отчеству\n" +
            "\\- школе\n" +
            "\\- городу\n" +
            "\\- юзернейму Telegram\n\n" +
            "Для поиска достаточно просто *написать запрос сообщением*\\.\n\n" +
            "🎓 *Поиск по классу и группе*\n" +
            "Для поиска по этим параметрам используются отдельные команды:\n" +
            "\\- `/class` — поиск по номеру класса\n" +
            "\\- `/group` — поиск по номеру группы\n\n" +
            "💡 *Для чего можно использовать бота*\n" +
            "С помощью бота вы можете:\n" +
            "\\- найти контактную информацию своих знакомых из лагеря\n" +
            "\\- найти данные вожатых лагеря\n" +
            "\\- понять, кто пишет сообщение в общем чате, если вы знаете только юзернейм телеграма\n"),
            replyMarkup: new ReplyKeyboardRemove(),
            parseMode: ParseMode.MarkdownV2
        );
        return false;
    }
}