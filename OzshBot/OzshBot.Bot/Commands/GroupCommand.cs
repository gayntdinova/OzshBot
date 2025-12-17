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
using System.ComponentModel.DataAnnotations;
namespace OzshBot.Bot;


public class GroupCommand : IBotCommand
{
    public string Name()
    =>"/group";

    public Role GetRole()
    =>Role.Child;

    public string GetDescription()
    =>"поиск пользователей по группе";

    public async Task<bool> ExecuteAsync(Update update, 
                                   ITelegramBotClient bot, 
                                   UserService userService)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                var message = update.Message!;
                var messageText = message.Text!;
                var username = message.From!.Username!;
                var userId = message.From.Id;
                var chat = message.Chat;
                var role = userService.RoleService.GetUserRoleByTgAsync(new TelegramInfo { TgUsername = username, TgId = userId }).Result;
                
                var splitted = messageText.Split(" ");
                if(splitted.Length!=2 || !Regex.IsMatch(splitted[1],@"^\d+$"))
                {
                    await bot.SendMessage(
                        chat.Id,
                        $"Использование: /group номер группы",
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                        );
                    return false;
                }

                var users = await userService.FindService.FindUsersByGroupAsync(int.Parse(splitted[1]));

                if (users.Length == 0)
                {
                    await bot.SendMessage(
                        chat.Id,
                        $"никто не найден",
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                        );
                }
                else if (users.Count() == 1)
                {
                    if(role == Role.Counsellor)
                        await bot.SendMessage(
                            chat.Id,
                            users[0].FormateAnswer(role),
                            replyMarkup: new InlineKeyboardMarkup(
                                InlineKeyboardButton.WithCallbackData("Редактировать", "edit "+users[0].PhoneNumber)
                            ),
                            parseMode: ParseMode.MarkdownV2
                            );
                    else
                        await bot.SendMessage(
                            chat.Id,
                            users[0].FormateAnswer(role),
                            replyMarkup: new ReplyKeyboardRemove(),
                            parseMode: ParseMode.MarkdownV2
                            );
                }
                else
                    await bot.SendMessage(
                        chat.Id,
                        users.FormateAnswer(messageText),
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                        );
                
                
                return false;
            default:
                return false;
        }
    }
}
