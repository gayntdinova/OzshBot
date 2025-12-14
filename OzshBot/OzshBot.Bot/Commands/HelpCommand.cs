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

    public async Task<bool> ExecuteAsync(Update update, 
                                   ITelegramBotClient bot, 
                                   UserService userService)
    {
        var message = update.Message!;
        var messageText = message.Text!;
        var username = message.From!.Username!;
        var userId = message.From.Id;
        var chat = message.Chat;
        var role = userService.RoleService.GetUserRoleByTgAsync(new TelegramInfo { TgUsername = username, TgId = userId }).Result;

        await bot.SendMessage(
            chat.Id,
            $"хелп",
            parseMode: ParseMode.MarkdownV2
            );
        return false;
    }
}