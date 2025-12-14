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


public class AddCommand : IBotCommand
{
    private readonly Dictionary<long,AddState> stateDict= new();
    public string Name()
    =>"/add";

    public Role GetRole()
    =>Role.Counsellor;

    public string GetDescription()
    =>"добавить нового пользователя";

    public async Task<bool> ExecuteAsync(Update update, 
                                   ITelegramBotClient bot, 
                                   UserService userService)
    {
        Console.WriteLine("---------------------------------------------------");
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
                    var attributeInfo = state.UserAttribute.GetInfo();
                    //если подходит под регулярку этого аттрибута то перекидываем на следующий или заканчиваем
                    if (Regex.IsMatch(messageText,attributeInfo.RegularExpression))
                    {
                        attributeInfo.FillingAction(state.AddUser,messageText);

                        var numberOfAttributes = Enum.GetNames(typeof(UserAttribute)).Length;

                        var counter = (int)state.UserAttribute + 1;
                        Console.WriteLine(counter);
                        while (counter < numberOfAttributes && !state.AddUser.Role.ImplementsAttribute((UserAttribute)counter))
                            counter+=1;

                        //если этот атрибут нашёлся(при условии что он есть у роли создаваемого человека)
                        if (counter < numberOfAttributes)
                        {
                            await SendStateInfoMessage(chat,bot,stateDict[userId],(UserAttribute)counter,false);
                            return true;
                        }
                        //если нет то заканчиваем пытаясь добавить пользователя и выводим его в чат
                        else
                        {
                            return await HandleAddUser(bot,chat,userService,userId,state);
                        }
                    }
                    //если не подходит под регулярку то переспрашиваем
                    else
                    {
                        await SendStateInfoMessage(chat,bot,stateDict[userId],state.UserAttribute,true);
                        return true;
                    }
                }
                //если нам написали /add
                else
                {
                    stateDict[userId] = new AddState();

                    var messageId = (await bot.SendMessage(
                        chat.Id,
                        "Начинаем создание пользователя, для отмены нажмите отмена",
                        replyMarkup: new InlineKeyboardMarkup(
                            InlineKeyboardButton.WithCallbackData("Отмена", "addCancel"))
                        )).Id;
                    stateDict[userId].messagesIds.Push(messageId);

                    await SendStateInfoMessage(chat,bot,stateDict[userId],UserAttribute.Role,false);
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

    private async Task<bool> HandleAddUser(ITelegramBotClient bot,Chat chat,UserService userService, long userId, AddState state)
    {
        UserDtoModel dto = (state.AddUser.Role==Role.Counsellor)?state.AddUser.ToCounsellorDto():state.AddUser.ToChildDto();
        var result = await userService.ManagementService.AddUserAsync(dto);
        if (result.IsFailed)
        {
            await bot.SendMessage(
                chat.Id,
                $"Не удалось добавить пользователя",
                parseMode: ParseMode.MarkdownV2
                );
            await TryCancelState(bot,chat,userId);
        }
        else
        {
            await bot.SendMessage(
                chat.Id,
                $"Пользователь успешно добавлен",
                parseMode: ParseMode.MarkdownV2
                );
            await TryCancelState(bot,chat,userId);
            await bot.SendMessage(
                chat.Id,
                state.AddUser.FormateAnswer(Role.Counsellor),
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData("Редактировать", "editMenu "+state.AddUser.PhoneNumber)
                ),
                parseMode: ParseMode.MarkdownV2
            );
        }
        return false;
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

    private async Task SendStateInfoMessage(Chat chat,ITelegramBotClient bot,AddState state,UserAttribute attribute, bool wasIncorrect)
    {
        var attributeInfo = attribute.GetInfo();
        var messageId = (await bot.SendMessage(
            chat.Id,
            ((wasIncorrect?"Некорректный формат\n":"")+attributeInfo.WritingInfo).FormateString(),
            parseMode: ParseMode.MarkdownV2
            )).Id;
        state.messagesIds.Push(messageId);
        state.UserAttribute = attribute;
    }

    class AddState
    {
        public UserAttribute UserAttribute = UserAttribute.Role;
        public UserDomain AddUser = new UserDomain{FullName = new(),PhoneNumber = ""};
        public Stack<MessageId> messagesIds = new();
    }
}

