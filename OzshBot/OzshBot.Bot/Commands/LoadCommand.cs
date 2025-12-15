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


public class LoadCommand : IBotCommandWithState
{
    private readonly Dictionary<long,AddState> stateDict= new();
    public string Name()
    =>"/load";

    public Role GetRole()
    =>Role.Counsellor;

    public string GetDescription()
    =>"Загрузить новых пользователей из таблицы в смену";

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

                //если студент то не может пользоваться этой командой
                if (role == Role.Child)
                {
                    await bot.SendMessage(
                        chat.Id,
                        "У вас нет прав пользоваться этой командой",
                        parseMode: ParseMode.MarkdownV2
                        );

                    await TryCancelState(bot,chat,userId);
                    return false;
                }

                //если уже находится в ожидании какого то ответа
                if(stateDict.TryGetValue(update.Message!.From!.Id, out var state))
                {
                    state.messagesIds.Push(update.Message.Id);
                    //todo проверку таблицы
                }
                //если нам написали /load
                else
                {
                    stateDict[userId] = new AddState();

                    var messageId = (await bot.SendMessage(
                        chat.Id,
                        "Начинаем добавление людей из таблицы",
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Отмена", "addCancel"))
                        )).Id;
                    stateDict[userId].messagesIds.Push(messageId);

                    messageId = (await bot.SendMessage(
                        chat.Id,
                        "Введите название смены, в которую вы хотите добавить пользователей из таблицы"
                        )).Id;
                    stateDict[userId].messagesIds.Push(messageId);
                    //todo клавиатуру
                }
                return true;

            case UpdateType.CallbackQuery:
                var callback = update.CallbackQuery!;
                
                //если нажали на любую кнопку то сразу заканчиваем
                await TryCancelState(bot,callback.Message!.Chat,callback.From.Id);

                return false;
            default:
                return false;
        }
    }

    private async Task<bool> HandleTable(Chat chat, Role role, string messageText)
    {
        if (role == Role.Child) return;

        var result = await userService.ManagementService.LoadTableAsync(messageText);
        if (result.IsFailed)
        {
            await botClient.SendMessage(
                chat.Id,
                "Не удалось загрузить таблицу",
                parseMode: ParseMode.MarkdownV2
                );
            return true
        }
        else
        {
            await botClient.SendMessage(
                chat.Id,
                "Таблица успешно загружена",
                parseMode: ParseMode.MarkdownV2
                );
            return false;
        }
    }

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
        public string Link;
        public Stack<MessageId> messagesIds = new();

        public LoadState(string link)
        {
            Link = link
        }
    }
}

