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


public class EditCommand : IBotCommand
{
    private readonly Dictionary<long,EditState> stateDict= new();
    public string Name()
    =>"edit";

    public Role GetRole()
    =>Role.Counsellor;

    public string GetDescription()
    =>"";

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
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                        );

                    await TryCancelState(bot,chat,userId);
                    return false;
                }

                //если уже находится в ожидании какого то ответа
                if(stateDict.TryGetValue(update.Message!.From!.Id, out var state))
                {
                    //если во время редактирования, то записываем сообщения в удаляемые
                    state.messagesIds.Push(message.Id);
                    state.MessagesToDelete += 1;

                    //если мы ждём нажатия на кнопку, то отменяем всё 
                    if (state.WaitingSelectField)
                    {
                        await TryCancelState(bot,chat,userId);
                        return false;
                    }


                    var attributeInfo = state.UserAttribute.GetInfo();

                    //если подходит под регулярку этого аттрибута то перекидываем на следующий или заканчиваем
                    if (await attributeInfo.CorrectFormateFunction(messageText))
                    {
                        state.MessagesToDelete = 0;
                        attributeInfo.FillingAction(state.EditUser,messageText);
                        state.WaitingSelectField = true;

                        state.messagesIds.Push((await bot.SendMessage( 
                            chat.Id,
                            "Изменено",
                            replyMarkup: new ReplyKeyboardRemove())).Id);
                        return true;
                    }
                    //если не подходит под регулярку то переспрашиваем
                    else
                    {
                        await SendStateInfoMessage(bot,chat,stateDict[userId],state.UserAttribute,true);
                        return true;
                    }
                }
                return false;

            case UpdateType.CallbackQuery:
                var callback = update.CallbackQuery!;
                var splitted = callback.Data!.Split();
                switch (splitted[0])
                {
                    case "edit":
                        return await HandleEditMenu(bot,userService, callback.Message!.Chat,callback.From.Id, splitted[1]);

                    case "editTheme":
                        return await HandleEditThemes(bot,callback.Message!.Chat,callback.From.Id,(UserAttribute)int.Parse(splitted[1]));

                    case "editApply":
                        return await HandleEditApply(bot,userService, callback.Message!.Chat,callback.From.Id);

                    default:
                        await TryCancelState(bot,callback.Message!.Chat,callback.From.Id);
                        return false;
                }
            default:
                return false;
        }
    }

    private async Task<bool> HandleEditMenu( ITelegramBotClient bot,UserService userService, Chat chat, long userId, string phoneNumber)
    {
        await TryCancelState(bot, chat, userId);

        var editedUser = await userService.FindService.FindUserByPhoneNumberAsync(phoneNumber);

        if (editedUser==null)
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
            var state = new EditState(editedUser);
            state.messagesIds.Push((await bot.SendMessage(
                chat.Id,
                "Выбирите что редактировать, если вы напишете сообщение не по теме, редактирование отменится",
                replyMarkup: CreateKeyboard(editedUser.Role),
                parseMode: ParseMode.MarkdownV2
            )).Id);
            
            stateDict[userId] = state;
            return true;
        }
    }

    private async Task<bool> HandleEditThemes(ITelegramBotClient bot, Chat chat, long userId, UserAttribute attribute)
    {
        if(!stateDict.Keys.Contains(userId)) return false;//невозможный в теории случай но я на всякий оставлю

        var state = stateDict[userId];
        if(!state.WaitingSelectField)
            while(state.MessagesToDelete > 0)
            {
                await bot.DeleteMessage(chat, state.messagesIds.Pop());
                state.MessagesToDelete-=1;
            }

        await SendStateInfoMessage(bot,chat,state,attribute,false);
        state.WaitingSelectField = false;
        return true;
    }

    private async Task<bool> HandleEditApply(ITelegramBotClient bot,UserService userService, Chat chat, long userId)
    {
        if(!stateDict.Keys.Contains(userId)) return false;//невозможный в теории случай но я на всякий оставлю

        var state = stateDict[userId];
        var result = await userService.ManagementService.EditUserAsync(state.EditUser);

        if (result.IsFailed)
        {
            await bot.SendMessage(
                chat.Id,
                $"Не удалось отредактировать пользователя пользователя",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
        }
        else
        {
            await bot.SendMessage(
                chat.Id,
                $"Пользователь успешно отредактирован",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
            await bot.SendMessage(
                chat.Id,
                state.EditUser.FormateAnswer(Role.Counsellor),
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Посещённые смены", "userSessions "+state.EditUser.PhoneNumber),
                    InlineKeyboardButton.WithCallbackData("Смены", "sessions "+state.EditUser.PhoneNumber)
                ),
                parseMode: ParseMode.MarkdownV2
            );
        }
        await TryCancelState(bot,chat,userId);
        return false;
    }

    private InlineKeyboardMarkup CreateKeyboard(Role role)
    {

        var result = new List<InlineKeyboardButton[]>();

        var editableWithRole = UserAttributesInfoManager.EditableAttributes.Where(attr=>role.ImplementsAttribute(attr)).ToArray();

        for(var i = 0;i<editableWithRole.Length;i+=2)
        {
            var attribute = UserAttributesInfoManager.EditableAttributes[i];

                var first = UserAttributesInfoManager.EditableAttributes[i];
                var second = UserAttributesInfoManager.EditableAttributes[i + 1];

                result.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData(first.GetInfo().Name,$"editTheme {(int)first}"),
                    InlineKeyboardButton.WithCallbackData(second.GetInfo().Name, $"editTheme {(int)second}")
                });
        }
        if (editableWithRole.Length % 2 == 1)
        {
            var last = editableWithRole[editableWithRole.Length-1];
            result.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(last.GetInfo().Name, $"editTheme {(int)last}")
            });
        }

        result.Add(new []
        {
            InlineKeyboardButton.WithCallbackData("Отмена", "editCancel")
        });
        result.Add(new []
        {
            InlineKeyboardButton.WithCallbackData("Применить", "editApply")
        });
        return result.ToArray();
    }


    private async Task TryCancelState(ITelegramBotClient bot,Chat chat,long userId)
    {
        if (stateDict.ContainsKey(userId))
        {
            while(stateDict[userId].messagesIds.Count!=0)
                await bot.DeleteMessage(chat, stateDict[userId].messagesIds.Pop());
            stateDict.Remove(userId);
        }
    }

    private async Task SendStateInfoMessage(ITelegramBotClient bot,Chat chat,EditState state,UserAttribute attribute, bool wasIncorrect)
    {
        var attributeInfo = attribute.GetInfo();
        ReplyMarkup markup = attributeInfo.KeyboardMarkup!=null?await attributeInfo.KeyboardMarkup(state.EditUser): new ReplyKeyboardRemove();

        state.messagesIds.Push((await bot.SendMessage(
            chat.Id,
            ((wasIncorrect?"Некорректный формат\n":"")+attributeInfo.WritingInfo).FormateString(),
            parseMode: ParseMode.MarkdownV2,
            replyMarkup: markup
            )).Id);
        state.UserAttribute = attribute;
        state.MessagesToDelete += 1;
    }

    class EditState
    {
        public bool WaitingSelectField = true;
        public UserAttribute UserAttribute;
        public UserDomain EditUser;
        public Stack<MessageId> messagesIds = new();
        public int MessagesToDelete = 0;

        public EditState(UserDomain editUser)
        {
            EditUser = editUser;
        }
    }
}

