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


public class SessionsCommand : IBotCommand
{
    private readonly Dictionary<long, SessionsState> stateDict = new();

    public string Name() => "/sessions";
    public Role GetRole() => Role.Counsellor;
    public string GetDescription() => "Добавить / Изменить смену";

    public async Task<bool> ExecuteAsync(
        Update update,
        ITelegramBotClient bot,
        UserService userService)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                return await HandleMessage(update.Message!, bot, userService);

            case UpdateType.CallbackQuery:
                return await HandleCallback(update.CallbackQuery!, bot, userService);

            default:
                return false;
        }
    }

    private async Task<bool> HandleMessage(
        Message message,
        ITelegramBotClient bot,
        UserService userService)
    {
        var chat = message.Chat;
        var userId = message.From!.Id;
        var text = message.Text!;

        var role = await userService.RoleService.GetUserRoleByTgAsync(
            new TelegramInfo { TgId = userId, TgUsername = message.From.Username! });

        if (role == Role.Child)
        {
            await bot.SendMessage(
                chat.Id,
                "У вас нет прав пользоваться этой командой",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );

            await TryCancelState(bot,chat,userId);
            return false;
        }

        if (stateDict.TryGetValue(userId, out var state))
        {
            state.messagesIds.Push(message.Id);
            state.MessagesToDelete += 1;

            if (state.Mode == SessionsMode.Edit)
            {
                //если мы нажали редактировать, ждём какое именно редактировать
                if(state.sessionToEdit == null)
                {
                    if (Regex.IsMatch(text,
                        @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}) (0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                    {
                        var splitted = text.Split(' ');
                        var start = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                        var end = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");

                        var sessions = await userService.SessionService.GetAllSessionsAsync();
                        if(sessions.Any(session=>session.SessionDates==new SessionDates(start, end)))
                        {
                            state.sessionToEdit = sessions.First(session=>session.SessionDates==new SessionDates(start, end));

                            state.messagesIds.Push((await bot.SendMessage(
                                chat.Id,
                                "Введите новые даты смены в формате dd.MM.yyyy dd.MM.yyyy".FormateString(),
                                parseMode: ParseMode.MarkdownV2,
                                replyMarkup: new ReplyKeyboardRemove())).Id);
                            state.MessagesToDelete += 1;
                        }
                        return true;
                    }
                    state.messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        "Выберите из предложенных",
                        parseMode: ParseMode.MarkdownV2,
                        replyMarkup: new ReplyKeyboardRemove())).Id);
                    state.MessagesToDelete += 1;
                    return true;
                }
                //если мы выбрали какую редактировать
                else
                {
                    if (Regex.IsMatch(text,
                        @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}) (0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                    {
                        var splitted = text.Split(' ');
                        var start = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                        var end = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");

                        state.sessionToEdit.SessionDates = new(start,end);
                        var result = await userService.SessionService.EditSessionAsync(state.sessionToEdit);

                        if (result.IsFailed)
                        {
                            await bot.SendMessage(
                                chat.Id,
                                result.Errors.First().GetExplanation(),
                                parseMode: ParseMode.MarkdownV2,
                                replyMarkup: new ReplyKeyboardRemove());   
                        }
                        else
                        {
                            await bot.SendMessage(
                                chat.Id,
                                "Смена успешно изменена",
                                parseMode: ParseMode.MarkdownV2,
                                replyMarkup: new ReplyKeyboardRemove());
                        }
                        await TryCancelState(bot,chat,userId);
                        return false;
                    }
                    else
                    {
                        state.messagesIds.Push((await bot.SendMessage(
                            chat.Id,
                            "Неверный формат даты",
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: new ReplyKeyboardRemove())).Id);
                        state.MessagesToDelete += 1;
                        return true;
                    }
                }

            }

            if (state.Mode == SessionsMode.Add)
            {
                var sessions = await userService.SessionService.GetAllSessionsAsync();
                if (Regex.IsMatch(text,
                    @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}) (0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                {
                    var splitted = text.Split(' ');
                    var start = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                    var end = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");
                    var result = await userService.SessionService.AddSessionAsync(new Session{SessionDates = new(start, end)});

                    if (result.IsFailed)
                    {
                        await bot.SendMessage(
                            chat.Id,
                            result.Errors.First().GetExplanation(),
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: new ReplyKeyboardRemove());   
                    }
                    else
                    {
                        await bot.SendMessage(
                            chat.Id,
                            "Смена успешно добавлена",
                            parseMode: ParseMode.MarkdownV2,
                            replyMarkup: new ReplyKeyboardRemove());
                    }
                    
                    await TryCancelState(bot, chat, userId);
                    return false;
                }
                else
                {
                    state.messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        "Неверный формат даты",
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2)).Id);
                    state.MessagesToDelete += 1;
                    return true;
                }
            }
            else
            {
                await TryCancelState(bot,chat,userId);
                return false;
            }
        }
        //если написали /sessions
        else
        {
            var sessions = await userService.SessionService.GetAllSessionsAsync();
            await TryCancelState(bot, chat, userId);

            stateDict[userId] = new SessionsState();
            stateDict[userId].messagesIds.Push((await bot.SendMessage(
                chat.Id,
                sessions.FormateAnswer(),
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Добавить", "sessionsAdd"),
                        InlineKeyboardButton.WithCallbackData("Редактировать", "sessionsEdit")
                    },
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Отмена", "sessionsCancel")
                    }
                }),
                parseMode: ParseMode.MarkdownV2
            )).Id);

            return true;
        }
        
        
    }


    private async Task<bool> HandleCallback(
        CallbackQuery callback,
        ITelegramBotClient bot,
        UserService userService)
    {
        var chat = callback.Message!.Chat;
        var userId = callback.From.Id;
        var data = callback.Data!;

        if (stateDict.TryGetValue(userId, out var state))
        {
            switch (data)
            {
                case "sessionsAdd":
                    while(state.MessagesToDelete > 0)
                    {
                        await bot.DeleteMessage(chat, state.messagesIds.Pop());
                        state.MessagesToDelete-=1;
                    }
                    state.sessionToEdit = null;
                    state.Mode = SessionsMode.Add;
                    state.messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        "Введите даты новой смены в формате dd.MM.yyyy dd.MM.yyyy".FormateString(),
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                    )).Id);
                    state.MessagesToDelete += 1;
                    return true;

                case "sessionsEdit":
                    while(state.MessagesToDelete > 0)
                    {
                        await bot.DeleteMessage(chat, state.messagesIds.Pop());
                        state.MessagesToDelete-=1;
                    }
                    state.Mode = SessionsMode.Edit;

                    state.messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        "Выберите смену для редактирования:".FormateString(),
                        replyMarkup: await GetSessionsKeyboard(userService.SessionService),
                        parseMode: ParseMode.MarkdownV2
                    )).Id);
                    state.MessagesToDelete += 1;
                    return true;

                default:
                    await TryCancelState(bot, chat, userId);
                    return false;
            }
        }
        return false;
    }

    private async Task<ReplyKeyboardMarkup> GetSessionsKeyboard(ISessionService sessionService)
        => new ReplyKeyboardMarkup(
            (await sessionService
                .GetAllSessionsAsync())
                    .Select(session=>new KeyboardButton[]{new KeyboardButton($"{session.SessionDates.StartDate.ToString("dd.MM.yyyy")} {session.SessionDates.EndDate.ToString("dd.MM.yyyy")}")}))
                {ResizeKeyboard = true};

    private async Task TryCancelState(ITelegramBotClient bot, Chat chat,long userId)
    {
        if (stateDict.ContainsKey(userId))
        {
            while(stateDict[userId].messagesIds.Count!=0)
                await bot.DeleteMessage(chat, stateDict[userId].messagesIds.Pop());
            stateDict.Remove(userId);
        }
    }

    enum SessionsMode
    {
        None,
        Edit,
        Add
    }

    class SessionsState
    {
        public Session? sessionToEdit = null; 
        public SessionsMode Mode = SessionsMode.None;
        public Stack<MessageId> messagesIds = new();
        public int MessagesToDelete = 0;
    }
}