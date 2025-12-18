using System.Text.RegularExpressions;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OzshBot.Bot.Commands;

public class GroupCommand : IBotCommand
{
    private readonly Role[] roles = [Role.Child, Role.Counsellor];

    public string Name
        => "/group";

    public bool IsAvailable(Role role)
    {
        return roles.Contains(role);
    }

    public string Description
        => "поиск пользователей по группе";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
        Update update)
    {
        var bot = botHandler.BotClient;
        var serviceManager = botHandler.ServiceManager;

        switch (update.Type)
        {
            case UpdateType.Message:
                var message = update.Message!;
                var messageText = message.Text!;
                var username = message.From!.Username!;
                var userId = message.From.Id;
                var chat = message.Chat;
                var role = serviceManager.RoleService
                    .GetUserRoleByTgAsync(new TelegramInfo { TgUsername = username, TgId = userId }).Result;

                var splitted = messageText.Split(" ");
                if (splitted.Length != 2 || !Regex.IsMatch(splitted[1], @"^\d+$"))
                {
                    await bot.SendMessage(
                        chat.Id,
                        $"Использование: /group номер группы",
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                    );
                    return false;
                }

                var users = await serviceManager.FindService.FindUsersByGroupAsync(int.Parse(splitted[1]));

                await botHandler.SendResultMessage(users, chat, userId, role, messageText);
                return false;
            default:
                return false;
        }
    }
}