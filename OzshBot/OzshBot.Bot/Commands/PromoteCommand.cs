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


public class PromoteCommand : IBotCommand
{
    public string Name()
    =>"/promote";

    public Role GetRole()
    =>Role.Counsellor;

    public string GetDescription()
    =>"Выдать права вожатого";

    public async Task<bool> ExecuteAsync(Update update, 
                                   ITelegramBotClient bot, 
                                   ServiseManager serviseManager)
    {
        var message = update.Message!;
        var messageText = message.Text!;
        var username = message.From!.Username!;
        var userId = message.From.Id;
        var chat = message.Chat;
        var role = serviseManager.RoleService.GetUserRoleByTgAsync(new TelegramInfo { TgUsername = username, TgId = userId }).Result;

        if (role == Role.Child)
        {
            await bot.SendMessage(
                chat.Id,
                "У вас нет прав пользоваться этой командой",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
            return false;
        }
        var splitted = messageText.Split(" ");

        if (splitted.Length !=2 || !Regex.IsMatch(splitted[1], @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"))
        {
            await bot.SendMessage(
                chat.Id,
                "Использование: /promote номер телефона",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
            return false;
        }

        var result = await serviseManager.RoleService.PromoteToCounsellorAsync(splitted[1]);
        if (result.IsFailed)
        {
            await bot.SendMessage(
                chat.Id,
                $"Не удалось повысить до вожатого",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
        }
        else
        {
            await bot.SendMessage(
                chat.Id,
                $"Пользователь успешно повышен до вожатого",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
        }

        return false;
    }
}