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
using OzshBot.Application.AppErrors;
namespace OzshBot.Bot;


public class LoadCommand : IBotCommand
{
    private readonly Dictionary<long,LoadState> stateDict= new();
    public string Name()
    =>"/load";

    public Role GetRole()
    =>Role.Counsellor;

    public string GetDescription()
    =>"Загрузить новых пользователей из таблицы в смену";

    public async Task<bool> ExecuteAsync(Update update, 
                                   ITelegramBotClient bot, 
                                   ServiseManager serviseManager)
    {
        switch (update.Type)
        {
            case UpdateType.Message:
                var message = update.Message!;
                var messageText = message.Text!;
                var username = message.From!.Username!;
                var userId = message.From.Id;
                var chat = message.Chat;
                var role = serviseManager.RoleService.GetUserRoleByTgAsync(new TelegramInfo { TgUsername = username, TgId = userId }).Result;

                //если студент то не может пользоваться этой командой
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

                var sessions = await serviseManager.SessionService.GetAllSessionsAsync();

                //если уже находится в ожидании какого то ответа
                if(stateDict.TryGetValue(update.Message!.From!.Id, out var state))
                {
                    state.messagesIds.Push(update.Message.Id);
                    if (state.SessionDates == null)
                    {
                        if (Regex.IsMatch(messageText,@"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}) (0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                        {
                            var splitted = messageText.Split(" ");
                            var startDate = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                            var endDate = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");
                            if (sessions.Any(session=>session.SessionDates.StartDate==startDate && session.SessionDates.EndDate == endDate))
                            {
                                state.SessionDates = new SessionDates(startDate,endDate);
                                state.messagesIds.Push((await bot.SendMessage(
                                    chat.Id,
                                    "Напишите url(ссылку) таблицы с учениками",
                                    replyMarkup: GetSessionsKeyboard(sessions)
                                    )).Id);
                                return true;
                            }
                            else
                            {
                                state.messagesIds.Push((await bot.SendMessage(
                                    chat.Id,
                                    "Такой сессии не существует, выберите из списка",
                                    replyMarkup: GetSessionsKeyboard(sessions)
                                    )).Id);
                                return true;
                            }
                        }
                        else
                        {
                            state.messagesIds.Push((await bot.SendMessage(
                                chat.Id,
                                "неправильный формат, верный формат: dd.MM.yyyy dd.MM.yyyy , но лучше просто нажать на вариант в клавиатуре",
                                replyMarkup: GetSessionsKeyboard(sessions)
                                )).Id);
                            return true;
                        }
                    }
                    else
                    {
                        var result = await serviseManager.ManagementService.LoadTableAsync(messageText,state.SessionDates);
                        if (result.IsFailed)
                        {
                            await bot.SendMessage(
                                chat.Id,
                                result.Errors.First().GetExplanation(),
                                replyMarkup: new ReplyKeyboardRemove(),
                                parseMode: ParseMode.MarkdownV2
                                );
                        }
                        else
                        {
                            await bot.SendMessage(
                                chat.Id,
                                "Таблица успешно загружена",
                                replyMarkup: new ReplyKeyboardRemove(),
                                parseMode: ParseMode.MarkdownV2
                                );
                            
                        }
                        await TryCancelState(bot,chat,userId);
                        return false;
                    }
                }
                //если нам написали /load
                else
                {
                    await TryCancelState(bot,chat,userId);
                    stateDict[userId] = new LoadState();

                    stateDict[userId].messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        "Начинаем добавление людей из таблицы",
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Отмена", "loadCancel"))
                        )).Id);

                    

                    stateDict[userId].messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        "Введите даты смены, в которую вы хотите добавить пользователей из таблицы",
                        replyMarkup: GetSessionsKeyboard(sessions)
                        )).Id);
                    return true;
                }

            case UpdateType.CallbackQuery:
                var callback = update.CallbackQuery!;
                
                //если нажали на любую кнопку то сразу заканчиваем
                await TryCancelState(bot,callback.Message!.Chat,callback.From.Id);

                return false;
            default:
                return false;
        }
    }

    private ReplyKeyboardMarkup GetSessionsKeyboard(Session[] sessions)
        => new ReplyKeyboardMarkup(sessions
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

    class LoadState
    {
        public SessionDates? SessionDates;
        public Stack<MessageId> messagesIds = new();
    }
}

