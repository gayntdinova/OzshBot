using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Ninject;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Formats.Asn1;
using UserDomain = OzshBot.Domain.Entities.User;
using UserTg = Telegram.Bot.Types.User;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using OzshBot.Application.Services;
using System.Data;
using System.Text.RegularExpressions;
using FluentResults;
using System.Net.Http.Headers;
using System.Windows.Input;
namespace OzshBot.Bot;


public class HelpCommand : IBotCommand
{
    public string Name()
    =>"/help";

    public Role GetRole()
    =>Role.Child;

    public string GetDescription()
    =>"Помощь";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
                                        Update update)
    {
        var bot = botHandler.botClient;
        var serviseManager = botHandler.serviseManager;
        
        var message = update.Message!;
        var messageText = message.Text!;
        var username = message.From!.Username!;
        var userId = message.From.Id;
        var chat = message.Chat;
        var role = serviseManager.RoleService.GetUserRoleByTgAsync(new TelegramInfo { TgUsername = username, TgId = userId }).Result;

        await bot.SendMessage(
            chat.Id,
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
            "\\- понять, кто пишет сообщение в общем чате, если вы знаете только юзернейм телеграма\n",
            replyMarkup: new ReplyKeyboardRemove(),
            parseMode: ParseMode.MarkdownV2
            );
        return false;
    }
}