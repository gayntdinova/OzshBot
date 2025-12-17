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
using System.Runtime.ExceptionServices;
namespace OzshBot.Bot;


public class UserSessionsCommand : IBotCommand
{
    public string Name()
    =>"userSessions";

    public Role GetRole()
    =>Role.Child;

    public string GetDescription()
    =>"";

    public async Task<bool> ExecuteAsync(Update update, 
                                   ITelegramBotClient bot, 
                                   ServiseManager serviseManager)
    {
        switch (update.Type)
        {
            case UpdateType.CallbackQuery:
                var callback = update.CallbackQuery!;
                var splitted = callback.Data!.Split();
                var chat = callback.Message!.Chat;

                var user = await serviseManager.FindService.FindUserByPhoneNumberAsync(splitted[1]);

                if (user==null)
                {
                    await bot.SendMessage(
                        chat.Id,
                        "Телефон этого человека сменился или его уже не существует",
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                        );
                    return false;
                }
                else
                {
                    var sessions = new Session[0];
                    if (user.ChildInfo != null)
                        sessions = sessions.Concat(user.ChildInfo.Sessions).ToArray();
                    if (user.CounsellorInfo != null)
                        sessions = sessions.Concat(user.CounsellorInfo.Sessions).ToArray();

                    await bot.SendMessage(
                        chat.Id,
                        sessions.FormateAnswer(user),
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                    );
                    return false;
                }
        }
        return false;
    }
}

