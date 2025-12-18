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


public class ProfileCommand : IBotCommand
{
    private readonly Role[] roles = new[]{Role.Child, Role.Counsellor};
    public string Name()
    =>"/profile";

    public bool IsAvailible(Role role)
    =>roles.Contains(role);

    public string GetDescription()
    =>"Мой профиль";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
                                        Update update)
    {
        var bot = botHandler.botClient;
        var serviceManager = botHandler.serviceManager;
        
        var message = update.Message!;
        var messageText = message.Text!;
        var username = message.From!.Username!;
        var userId = message.From.Id;
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