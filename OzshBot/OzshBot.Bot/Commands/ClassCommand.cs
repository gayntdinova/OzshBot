using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text.RegularExpressions;
namespace OzshBot.Bot;


public class ClassCommand : IBotCommand
{
    private readonly Role[] roles = new[]{Role.Child, Role.Counsellor};
    public string Name
    => "/class";

    public bool IsAvailable(Role role)
    => roles.Contains(role);

    public string Description
    => "поиск пользователей по классу";

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
                var role = serviceManager.RoleService.GetUserRoleByTgAsync(new TelegramInfo { TgUsername = username, TgId = userId }).Result;
                
                var splitted = messageText.Split(" ");
                if(splitted.Length!=2 || !Regex.IsMatch(splitted[1],@"^(?:[1-9]|10|11)$"))
                {
                    await bot.SendMessage(
                        chat.Id,
                        $"Использование: /class номер класса",
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                        );
                    return false;
                }

                var users = await serviceManager.FindService.FindUsersByClassAsync(int.Parse(splitted[1]));

                await botHandler.SendResultMessage(users,chat,userId,role, messageText);
                
                return false;
            default:
                return false;
        }
    }
}
