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
namespace OzshBot.Bot;

class BotHandler
{
    //клиент для работы с Bot API, позволяет отправлять сообщения, управлять ботом, подписываться на обновления и тд
    private ITelegramBotClient botClient;

    //объект с настройками бота, здесь указываем какие типы Update будем получать, Timeout бота и тд
    private ReceiverOptions receiverOptions;

    //сервис для работы с правами
    private UserService userService;

    //список в каком состоянии какие человек
    private Dictionary<long,State> stateDict = new();

    public BotHandler(
        ITelegramBotClient botClient,
        ReceiverOptions receiverOptions,
        UserService userService)
    {
        this.botClient = botClient;
        this.receiverOptions = receiverOptions;
        this.userService = userService;
    }

    //================================CoolMethods===================================
    public async Task Start()
    {
        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

        Console.WriteLine($"{(await botClient.GetMe()).FirstName} запущен!");

        await Task.Delay(-1);
    }

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    await HandleMessage(update);
                    break;

                case UpdateType.CallbackQuery:
                    await HandleCallbackQuery(update);
                    break;

                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        Console.WriteLine(error switch
        {
            _ => error.ToString()
        });

        return Task.CompletedTask;
    }

    //================================MessageMethods=================================
    private async Task HandleMessage(Update update)
    {
        if (update.Message is not { } message) return;
        if (message.Text is not { } messageText) return;
        if (message.From == null) return;//хз
        if (message.From.Username == null) return;//хз
        var splittedMessage = messageText.Split();

        var role = userService.RoleService.GetUserRoleByTgAsync(
            new TelegramInfo { TgUsername = message.From.Username, TgId = message.From.Id }).Result;
        var chat = message.Chat;

        Console.WriteLine($"id: {message.From.Id}\nusername {message.From.Username}\nроль: {role}"+(stateDict.Keys.Contains(message.From.Id)?("\nсостояние: "+ stateDict[message.From.Id].StateName):""));

        //==================================================================================================

        await SetCommandsForUser(role,message.From.Id);

        if(role == Role.Unknown)
            await botClient.SendMessage(
                chat.Id,
                "для того чтобы пользоваться этим ботом вы должны быть учавстником лагеря ОЗШ",
                parseMode: ParseMode.MarkdownV2
            );

        if (stateDict.Keys.Contains(message.From.Id))
        {
            await HandleMessageIfState(chat,role,message,message.From.Id);
            return;
        }

        switch (splittedMessage[0])
        {
            case "/start":
                break;

            case "/help":
                await HandleHelp(chat);
                break;

            case "/profile":
                await HandleProfile(chat,role,message.From.Username);
                break;

            case "/promote":
                await HandlePromote(chat,role,messageText);
                break;

            case "/delete":
                await HandleDelete(chat,role,messageText);
                break;

            case "/add":
                await HandleAdd(chat,role,message.From.Id);
                break;

            default:
                await HandleTable(chat,role,messageText);
                await HandleUsersSearching(chat,role,messageText);
                break;
        }
    }

    private async Task HandleHelp(Chat chat)
    {
        await botClient.SendMessage(
            chat.Id,
            $"",//todo
            parseMode: ParseMode.MarkdownV2
            );
    }

    private async Task HandleMessageIfState(Chat chat,Role role, Message message,long userId)
    {
        if (message.Text is not { } messageText) return;
        //todo что то с ролью

        var state = stateDict[userId];

        switch (state.StateName)
        {
            case UserState.EditingUser_SelectField:
            case UserState.EditingUser_WaitingFullName:
            case UserState.EditingUser_WaitingTgUsername:
            case UserState.EditingUser_WaitingBirthday:
            case UserState.EditingUser_WaitingCity:
            case UserState.EditingUser_WaitingPhoneNumber:
            case UserState.EditingUser_WaitingEmail:
            case UserState.EditingUser_WaitingClass:
            case UserState.EditingUser_WaitingSchool:
            case UserState.EditingUser_WaitingGroup:
                await HandleMessageIfEditingState(chat,message,userId);
                return;

            case UserState.CreatingUser_WaitingRole:
            case UserState.CreatingUser_WaitingPhoneNumber:
            case UserState.CreatingUser_WaitingFullName:
            case UserState.CreatingUser_WaitingTgUsername:
            case UserState.CreatingUser_WaitingEmail:
            case UserState.CreatingUser_WaitingBirthday:
            case UserState.CreatingUser_WaitingCity:
            case UserState.CreatingUser_WaitingGroup:
            case UserState.CreatingUser_WaitingSchool:
            case UserState.CreatingUser_WaitingClass:
                await HandleMessageIfCreatingState(chat,message,userId);
                return;
        }
    }

    private async Task HandleMessageIfEditingState(Chat chat, Message message,long userId)
    {
        if (message.Text is not { } messageText) return;

        var state = stateDict[userId];

        switch (state.StateName)
        {
            case UserState.EditingUser_SelectField:
                await CancelState(chat,userId);
                break;

            case UserState.EditingUser_WaitingFullName:
                state.messagesIds.Push(message.Id);
                
                if (Regex.IsMatch(messageText, @"^[А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+$"))
                {
                    state.numberOfDeletable = 0;
                    var splittedMessage = messageText.Split(" ");
                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.FullName.Surname = splittedMessage[0];
                    state.Data.FullName.Name = splittedMessage[1];
                    state.Data.FullName.Patronymic = splittedMessage[2];
                }
                else
                {
                    state.numberOfDeletable+=2;
                    await SendStateInfoMessage(state,chat,
                        "Некорректный формат\nВведите новое имя полностью (корректный формат: Фамилия Имя Отчество)",
                        UserState.EditingUser_WaitingFullName);
                }
                return;

            case UserState.EditingUser_WaitingTgUsername:
                state.messagesIds.Push(message.Id);
                if (Regex.IsMatch(messageText, @"^@[A-Za-z0-9_]+$"))
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    if (state.Data.TelegramInfo == null)
                        state.Data.TelegramInfo = new TelegramInfo{TgUsername = ""};
                    state.Data.TelegramInfo.TgUsername = messageText.Substring(1);
                }
                else
                {
                    state.numberOfDeletable+=2;
                    await SendStateInfoMessage(state,chat,
                        "Некорректный формат\nВведите новый юзернейм телеграма(корректный формат: @username)",
                        UserState.EditingUser_WaitingTgUsername);
                }
                return;

            case UserState.EditingUser_WaitingBirthday:
                state.messagesIds.Push(message.Id);
                if(messageText == "_")
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.Birthday = null;
                }
                else if (Regex.IsMatch(messageText, @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.Birthday = DateOnly.ParseExact(messageText, "dd.MM.yyyy");
                }
                else
                {
                    state.numberOfDeletable+=2;
                    await SendStateInfoMessage(state,chat,
                        "Некорректный формат\nВведите новый день рождения(корректный формат: день.месяц.год)",
                        UserState.EditingUser_WaitingBirthday);
                }
                return;

            case UserState.EditingUser_WaitingCity:
                state.messagesIds.Push(message.Id);

                if(messageText == "_")
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.City = null;
                }
                else
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.City = messageText;
                }
                
                return;

            case UserState.EditingUser_WaitingPhoneNumber:
                state.messagesIds.Push(message.Id);
                if (Regex.IsMatch(messageText, @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"))
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.PhoneNumber = messageText;
                }
                else
                {
                    state.numberOfDeletable+=2;
                    await SendStateInfoMessage(state,chat,
                        "Некорректный формат\nВведите новый номер телефона(корректный формат: +7**********)",
                        UserState.EditingUser_WaitingPhoneNumber);
                }
                return;
                
            case UserState.EditingUser_WaitingEmail:
                state.messagesIds.Push(message.Id);

                if(messageText == "_")
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.Email = null;
                }
                else if (Regex.IsMatch(messageText, @"^[\w\.\-]+@[\w\-]+\.[A-Za-z]{2,}$"))
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.Email = messageText;
                }
                else
                {
                    state.numberOfDeletable+=2;
                    await SendStateInfoMessage(state,chat,
                        "Некорректный формат\nВведите новый email(корректный формат: чтото@чтото.чтото)",
                        UserState.EditingUser_WaitingEmail);
                }
                return;

            case UserState.EditingUser_WaitingClass:
                state.messagesIds.Push(message.Id);
                if (Regex.IsMatch(messageText, @"^(?:[1-9]|10|11)$"))
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    if (state.Data.ChildInfo!=null)
                        state.Data.ChildInfo.EducationInfo.Class =int.Parse(messageText);
                }
                else
                {
                    state.numberOfDeletable+=2;
                    await SendStateInfoMessage(state,chat,
                        "Некорректный формат\nВведите новый номер класса(год обучения)",
                        UserState.EditingUser_WaitingClass);
                }
                return;

            case UserState.EditingUser_WaitingSchool:
                state.messagesIds.Push(message.Id);

                state.numberOfDeletable = 0;
                state.StateName = UserState.EditingUser_SelectField;
                if (state.Data.ChildInfo!=null)
                    state.Data.ChildInfo.EducationInfo.School = messageText;
                return;

            case UserState.EditingUser_WaitingGroup:
                state.messagesIds.Push(message.Id);
                if(messageText == "_")
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    if (state.Data.ChildInfo!=null)
                        state.Data.ChildInfo.Group = null;
                    if (state.Data.CounsellorInfo!=null)
                        state.Data.CounsellorInfo.Group = null;
                }
                else if (Regex.IsMatch(messageText, @"^\d+$"))
                {
                    state.numberOfDeletable = 0;
                    state.StateName = UserState.EditingUser_SelectField;
                    if (state.Data.ChildInfo!=null)
                        state.Data.ChildInfo.Group = int.Parse(messageText);
                    if (state.Data.CounsellorInfo!=null)
                        state.Data.CounsellorInfo.Group = int.Parse(messageText);
                }
                else
                {
                    state.numberOfDeletable+=2;
                    await SendStateInfoMessage(state,chat,
                        "Некорректный формат\nВведите новый номер группы",
                        UserState.EditingUser_WaitingGroup);
                }
                return;
        }
    }

    private async Task HandleMessageIfCreatingState(Chat chat,Message message, long userId)
    {
        if (message.Text is not { } messageText) return;

        var state = stateDict[userId];

        switch (state.StateName)
        {
            case UserState.CreatingUser_WaitingRole:
                state.messagesIds.Push(message.Id);

                if (messageText == "ребёнок" || messageText == "вожатый")
                {
                    if(messageText == "ребёнок")
                    {
                        state.Data.Role = Role.Child;
                        state.Data.ChildInfo = new();
                        state.Data.ChildInfo.EducationInfo = new EducationInfo
                        {
                            School = "",
                            Class = 0
                        };
                    }
                    else
                    {
                        state.Data.Role = Role.Counsellor;
                        state.Data.CounsellorInfo = new();
                    }

                    await SendStateInfoMessage(state,chat,
                        $"Введите номер телефона(корректный формат: +7**********)",
                        UserState.CreatingUser_WaitingPhoneNumber);
                }
                else
                    await SendStateInfoMessage(state,chat,
                        $"Некорректный формат\nНапишите какой роли человека вы хотите создать: `ребёнок` или `вожатый`",
                        UserState.CreatingUser_WaitingRole);
                break;

            case UserState.CreatingUser_WaitingPhoneNumber:
                state.messagesIds.Push(message.Id);

                if (Regex.IsMatch(messageText, @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"))
                {
                    state.Data.PhoneNumber = messageText;

                    await SendStateInfoMessage(state,chat,
                        $"Введите полное имя полностью (корректный формат: Фамилия Имя Отчество)",
                        UserState.CreatingUser_WaitingFullName);
                }
                else
                    await SendStateInfoMessage(state,chat,
                        $"Некорректный формат\nВведите номер телефона(корректный формат: +7**********)",
                        UserState.CreatingUser_WaitingPhoneNumber);
                break;

            case UserState.CreatingUser_WaitingFullName:
                state.messagesIds.Push(message.Id);

                if (Regex.IsMatch(messageText, @"^[А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+$"))
                {
                    var splittedMessage = messageText.Split(" ");
                    state.Data.FullName.Surname = splittedMessage[0];
                    state.Data.FullName.Name = splittedMessage[1];
                    state.Data.FullName.Patronymic = splittedMessage[2];

                    await SendStateInfoMessage(state,chat,
                        $"Введите юзернейм телеграма(корректный формат: @username)",
                        UserState.CreatingUser_WaitingTgUsername);
                }
                else
                    await SendStateInfoMessage(state,chat,
                        $"Некорректный формат\nВведите полное имя полностью (корректный формат: Фамилия Имя Отчество)",
                        UserState.CreatingUser_WaitingFullName);
                break;

            case UserState.CreatingUser_WaitingTgUsername:
                state.messagesIds.Push(message.Id);

                if (Regex.IsMatch(messageText, @"^@[A-Za-z0-9_]+$"))
                {
                    state.Data.TelegramInfo = new TelegramInfo{TgUsername = messageText.Substring(1)};

                    await SendStateInfoMessage(state,chat,
                        $"Введите email(корректный формат: чтото@чтото.чтото)",
                        UserState.CreatingUser_WaitingEmail);
                }
                else
                    await SendStateInfoMessage(state,chat,
                        $"Некорректный формат\nВведите юзернейм телеграма(корректный формат: @username)",
                        UserState.CreatingUser_WaitingTgUsername);
                break;

            case UserState.CreatingUser_WaitingEmail:
                state.messagesIds.Push(message.Id);

                if(messageText == "_")
                {
                    state.Data.Email = null;
                    await SendStateInfoMessage(state,chat,
                        $"Введите день рождения(корректный формат: день.месяц.год)",
                        UserState.CreatingUser_WaitingBirthday);
                }
                else if (Regex.IsMatch(messageText, @"^[\w\.\-]+@[\w\-]+\.[A-Za-z]{2,}$"))
                {
                    state.Data.Email = messageText;
                    await SendStateInfoMessage(state,chat,
                        $"Введите день рождения(корректный формат: день.месяц.год)",
                        UserState.CreatingUser_WaitingBirthday);
                }
                else
                    await SendStateInfoMessage(state,chat,
                        $"Некорректный формат\nВведите email(корректный формат: чтото@чтото.чтото)",
                        UserState.CreatingUser_WaitingEmail);
                break;

            case UserState.CreatingUser_WaitingBirthday:
                state.messagesIds.Push(message.Id);

                if(messageText == "_")
                {
                    state.Data.Birthday = null;
                    await SendStateInfoMessage(state,chat,
                        $"Введите название города",
                        UserState.CreatingUser_WaitingCity);
                }
                else if (Regex.IsMatch(messageText, @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                {
                    state.Data.Birthday = DateOnly.ParseExact(messageText, "dd.MM.yyyy");
                    await SendStateInfoMessage(state,chat,
                        $"Введите название города",
                        UserState.CreatingUser_WaitingCity);
                }
                else
                    await SendStateInfoMessage(state,chat,
                        $"Некорректный формат\nВведите день рождения(корректный формат: день.месяц.год)",
                        UserState.CreatingUser_WaitingBirthday);
                break;

            case UserState.CreatingUser_WaitingCity:
                state.messagesIds.Push(message.Id);

                if(messageText == "_")
                {
                    state.Data.City = null;
                    await SendStateInfoMessage(state,chat,
                        $"Введите номер группы",
                        UserState.CreatingUser_WaitingGroup);
                }
                else
                {
                    state.Data.City = messageText;
                    await SendStateInfoMessage(state,chat,
                        $"Введите номер группы",
                        UserState.CreatingUser_WaitingGroup);
                }
                break;

            case UserState.CreatingUser_WaitingGroup:
                state.messagesIds.Push(message.Id);

                if(messageText == "_")
                {
                    if (state.Data.ChildInfo!=null)
                        state.Data.ChildInfo.Group = null;
                    if (state.Data.CounsellorInfo!=null)
                        state.Data.CounsellorInfo.Group = null;

                    if(state.Data.Role == Role.Child)
                    {
                        await SendStateInfoMessage(state,chat,
                            $"Введите название школы",
                            UserState.CreatingUser_WaitingSchool);
                    }
                    else
                    {
                        var result = await userService.ManagementService.AddUserAsync(state.Data.ToCounsellorDto());
                        if (result.IsFailed)
                        {
                            await botClient.SendMessage(
                                chat.Id,
                                $"Не удалось добавить пользователя",
                                parseMode: ParseMode.MarkdownV2
                                );
                        }
                        else
                        {
                            await botClient.SendMessage(
                                chat.Id,
                                $"Пользователь успешно добавлен",
                                parseMode: ParseMode.MarkdownV2
                                );
                        }
                        await CancelState(chat,userId);
                    }
                        
                }
                else if (Regex.IsMatch(messageText, @"^\d+$"))
                {
                    if (state.Data.ChildInfo!=null)
                        state.Data.ChildInfo.Group = int.Parse(messageText);
                    if (state.Data.CounsellorInfo!=null)
                        state.Data.CounsellorInfo.Group = int.Parse(messageText);

                    if(state.Data.Role == Role.Child)
                    {
                        await SendStateInfoMessage(state,chat,
                            $"Введите название школы",
                            UserState.CreatingUser_WaitingSchool);
                    }
                    else
                    {
                        var result = await userService.ManagementService.AddUserAsync(state.Data.ToCounsellorDto());
                        if (result.IsFailed)
                        {
                            await botClient.SendMessage(
                                chat.Id,
                                $"Не удалось добавить пользователя",
                                parseMode: ParseMode.MarkdownV2
                                );
                        }
                        else
                        {
                            await botClient.SendMessage(
                                chat.Id,
                                $"Пользователь успешно добавлен",
                                parseMode: ParseMode.MarkdownV2
                                );
                        }   
                        await CancelState(chat,userId);
                    }
                }
                else
                    await SendStateInfoMessage(state,chat,
                        $"Некорректный формат\nВведите номер группы",
                        UserState.CreatingUser_WaitingGroup);
                break;

            case UserState.CreatingUser_WaitingSchool:
                state.messagesIds.Push(message.Id);
                if(state.Data.ChildInfo == null) return;

                state.Data.ChildInfo.EducationInfo.School = messageText;
                await SendStateInfoMessage(state,chat,
                    $"Введите номер класса",
                    UserState.CreatingUser_WaitingClass);
                break;

            case UserState.CreatingUser_WaitingClass:
                state.messagesIds.Push(message.Id);
                if(state.Data.ChildInfo == null) return;

                if (Regex.IsMatch(messageText, @"^(?:[1-9]|10|11)$"))
                {
                    state.Data.ChildInfo.EducationInfo.Class =int.Parse(messageText);
                    var result = await userService.ManagementService.AddUserAsync(state.Data.ToChildDto());
                    if (result.IsFailed)
                    {
                        await botClient.SendMessage(
                            chat.Id,
                            $"Не удалось добавить пользователя",
                            parseMode: ParseMode.MarkdownV2
                            );
                    }
                    else
                    {
                        await botClient.SendMessage(
                            chat.Id,
                            $"Пользователь успешно добавлен",
                            parseMode: ParseMode.MarkdownV2
                            );
                    }
                    await CancelState(chat,userId);
                }
                else
                    await SendStateInfoMessage(state,chat,
                        "Некорректный формат\nВведите новый номер класса(год обучения)",
                        UserState.CreatingUser_WaitingClass);
                    
                break;
        }
    }

    private async Task HandleProfile(Chat chat, Role role, string username)
    {
        var you = await userService.FindService.FindUserByTgAsync(new TelegramInfo{
            TgUsername = username,
            TgId = null});
        if(you.IsFailed)
            await botClient.SendMessage(
                chat.Id,
                $"вас не существует",
                parseMode: ParseMode.MarkdownV2
                );
        else
            await botClient.SendMessage(
                chat.Id,
                you.Value.FormateAnswer(role),
                parseMode: ParseMode.MarkdownV2
                );
    }

    private async Task HandlePromote(Chat chat, Role role, string messageText)
    {
        if (role == Role.Child)
        {
            await botClient.SendMessage(
                chat.Id,
                "У вас нет прав пользоваться этой командой",
                parseMode: ParseMode.MarkdownV2
                );
            return;
        }
        var splitted = messageText.Split(" ");

        if (splitted.Length !=2 || !Regex.IsMatch(splitted[1], @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"))
        {
            await botClient.SendMessage(
                chat.Id,
                "Введённая строка не является номером телефона",
                parseMode: ParseMode.MarkdownV2
                );
            return;
        }

        var result = await userService.RoleService.PromoteToCounsellorAsync(splitted[1]);
        if (result.IsFailed)
        {
            await botClient.SendMessage(
                chat.Id,
                $"Не удалось повысить до вожатого",
                parseMode: ParseMode.MarkdownV2
                );
        }
        else
        {
            await botClient.SendMessage(
                chat.Id,
                $"Пользователь успешно повышен до вожатого",
                parseMode: ParseMode.MarkdownV2
                );
        }
    }

    private async Task HandleDelete(Chat chat, Role role, string messageText)
    {
        if (role == Role.Child)
        {
            await botClient.SendMessage(
                chat.Id,
                "У вас нет прав пользоваться этой командой",
                parseMode: ParseMode.MarkdownV2
                );
            return;
        }
        var splitted = messageText.Split(" ");

        if (splitted.Length !=2 || !Regex.IsMatch(splitted[1], @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"))
        {
            await botClient.SendMessage(
                chat.Id,
                "Введённая строка не является номером телефона",
                parseMode: ParseMode.MarkdownV2
                );
            return;
        }

        var result = await userService.ManagementService.DeleteUserAsync(splitted[1]);
        if (result.IsFailed)
        {
            await botClient.SendMessage(
                chat.Id,
                $"Не удалось удалить пользователя",
                parseMode: ParseMode.MarkdownV2
                );
        }
        else
        {
            await botClient.SendMessage(
                chat.Id,
                $"Пользователь успешно удалён",
                parseMode: ParseMode.MarkdownV2
                );
        }
    }

    private async Task HandleAdd(Chat chat,Role role, long userId)
    {
        if (role == Role.Child)
        {
            await botClient.SendMessage(
                chat.Id,
                "У вас нет прав пользоваться этой командой",
                parseMode: ParseMode.MarkdownV2
                );
            return;
        }

        stateDict[userId] = new State(UserState.CreatingUser_WaitingRole);

        var messageId = (await botClient.SendMessage(
            chat.Id,
            "Начинаем создание пользователя, для отмены нажмите отмена",
            replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData("Отмена", "addCancel")
                    ),
            parseMode: ParseMode.MarkdownV2
            )).Id;
        stateDict[userId].messagesIds.Push(messageId);

        await SendStateInfoMessage(stateDict[userId],chat,
            "Напишите какой роли человека вы хотите создать: `ребёнок` или `вожатый`",
            UserState.CreatingUser_WaitingRole);
    }

    private async Task HandleTable(Chat chat, Role role, string messageText)
    {
        if (role == Role.Child) return;

        var result = await userService.ManagementService.LoadTableAsync(messageText);
        if (result.IsSuccess)
        {
            await botClient.SendMessage(
                chat.Id,
                "Таблица успешно загружена",
                parseMode: ParseMode.MarkdownV2
                );
        }
    }

    private async Task HandleUsersSearching(Chat chat, Role role, string messageText)
    {
        var result = await userService.FindService.FindUserAsync(messageText);

                switch (result)
                {
                    case {IsSuccess:true}:
                        var users = result.Value;
                        if (users.Count() == 1)
                        {
                            if(role == Role.Counsellor)
                                await botClient.SendMessage(
                                    chat.Id,
                                    users[0].FormateAnswer(role),
                                    replyMarkup: new InlineKeyboardMarkup(
                                        InlineKeyboardButton.WithCallbackData("Редактировать", "editMenu "+users[0].PhoneNumber)
                                    ),
                                    parseMode: ParseMode.MarkdownV2
                                    );
                            else
                                await botClient.SendMessage(
                                    chat.Id,
                                    users[0].FormateAnswer(role),
                                    parseMode: ParseMode.MarkdownV2
                                    );
                        }
                        else
                            await botClient.SendMessage(
                                chat.Id,
                                users.FormateAnswer(messageText),
                                parseMode: ParseMode.MarkdownV2
                                );
                        break;

                    case{IsFailed:true}:
                        await botClient.SendMessage(
                            chat.Id,
                            $"никто не найден",
                            parseMode: ParseMode.MarkdownV2
                            );
                        break;
                }
    }

    //================================CallbackQueryMethods============================
    private async Task HandleCallbackQuery(Update update)
    {
        if (update.CallbackQuery is not { } callback) return;
        if (callback.Message == null) return;//хз
        if (callback.From == null) return;//хз
        if (callback.From.Username == null)return;//хз
        if (callback.Data == null) return;//хз

        var userId = callback.From.Id;
        var chat = callback.Message.Chat;
        var splittedCommand = callback.Data.Split();

        var role = userService.RoleService.GetUserRoleByTgAsync(
            new TelegramInfo { TgUsername = callback.From.Username, TgId = callback.From.Id }).Result;

        if(role == Role.Unknown)
            await botClient.SendMessage(
                chat.Id,
                "для того чтобы пользоваться этим ботом вы должны быть учавстником лагеря ОЗШ",
                parseMode: ParseMode.MarkdownV2
            );

        switch (splittedCommand[0])
        {
            case "editMenu":
                await HandleEditMenu(chat,userId,splittedCommand[1]);
                break;

            case "editTheme":
                await HandleEditThemes(chat,userId,splittedCommand[1]);
                break;

            case "editCancel":
                await CancelState(chat,userId);
                break;
            
            case "editApply":
                await HandleEditApply(chat,userId);
                break;

            case "addCancel":
                await CancelState(chat,userId);
                break;
        }

        await botClient.AnswerCallbackQuery(callback.Id);
    }

    private async Task HandleEditMenu(Chat chat, long userId, string phoneNumber)
    {
        if(stateDict.Keys.Contains(userId))
            await CancelState(chat, userId);

        var redactedUser = await userService.FindService.FindUserByPhoneNumberAsync(phoneNumber);

        if (redactedUser.IsFailed)
        {
            await botClient.SendMessage(
                chat.Id,
                "Телефон этого человека сменился или его уже не существует",
                parseMode: ParseMode.MarkdownV2
                );
        }
        else
        {
            var messageId = (await botClient.SendMessage(
            chat.Id,
            "Выбирите что редактировать, если вы напишете сообщение не по теме редактирование отменится",
            replyMarkup: new InlineKeyboardMarkup(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Имя", "editTheme name"),
                        InlineKeyboardButton.WithCallbackData("Телеграм", "editTheme telegram")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("День Рождения", "editTheme birth"),
                        InlineKeyboardButton.WithCallbackData("Город", "editTheme city")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Телефон", "editTheme phone"),
                        InlineKeyboardButton.WithCallbackData("Email", "editTheme email")
                    }
                }.Concat(redactedUser.Value.Role==Role.Child?
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Класс", "editTheme class"),
                        InlineKeyboardButton.WithCallbackData("Школа", "editTheme school"),
                    }
                }:new InlineKeyboardButton[0][]).Concat(
                new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Группа", "editTheme group"),
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Отмена", "editCancel")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Применить", "editApply")
                    }

                }).ToArray()
                ),
                parseMode: ParseMode.MarkdownV2
            )).Id;
            var newState = new State(UserState.EditingUser_SelectField, redactedUser.Value);
            newState.messagesIds.Push(messageId);
            stateDict[userId] = newState;
        }
    }

    private async Task HandleEditThemes(Chat chat, long userId, string command)
    {
        if(!stateDict.Keys.Contains(userId)) return;

        var state = stateDict[userId];
        if(state.StateName != UserState.EditingUser_SelectField)
            while(state.numberOfDeletable > 0)
            {
                await botClient.DeleteMessage(chat, state.messagesIds.Pop());
                state.numberOfDeletable-=1;
            }

        switch (command)
        {
            case "name":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новое имя полностью (корректный формат: Фамилия Имя Отчество)",
                    UserState.EditingUser_WaitingFullName);
                break;
            case "telegram":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новый юзернейм телеграма(корректный формат: @username)",
                    UserState.EditingUser_WaitingTgUsername);
                break;
            case "birth":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новый день рождения(корректный формат: день.месяц.год) или введите _ если хотите чтобы этой информации не было",
                    UserState.EditingUser_WaitingBirthday);
                break;
            case "city":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новое название города или введите _ если хотите чтобы этой информации не было",
                    UserState.EditingUser_WaitingCity);
                break;
            case "phone":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новый номер телефона(корректный формат: +7**********)",
                    UserState.EditingUser_WaitingPhoneNumber);
                break;
            case "email":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новый email(корректный формат: чтото@чтото.чтото) или введите _ если хотите чтобы этой информации не было",
                    UserState.EditingUser_WaitingEmail);
                break;
            case "class":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новый номер класса(год обучения)",
                    UserState.EditingUser_WaitingClass);
                break;
            case "school":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новое название школы",
                    UserState.EditingUser_WaitingSchool);
                break;
            case "group":
                state.numberOfDeletable+=1;
                await SendStateInfoMessage(state,chat,
                    "Введите новый номер группы или введите _ если хотите чтобы этой информации не было",
                    UserState.EditingUser_WaitingGroup);
                break;
        }
    }

    private async Task HandleEditApply(Chat chat, long userId)
    {
        if(!stateDict.Keys.Contains(userId)) return;

        var state = stateDict[userId];
        await userService.ManagementService.EditUserAsync(state.Data);
        await CancelState(chat,userId);
        await botClient.SendMessage(
            chat.Id,
            state.Data.FormateAnswer(Role.Counsellor),
            replyMarkup: new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData("Редактировать", "editMenu "+state.Data.PhoneNumber)
            ),
            parseMode: ParseMode.MarkdownV2
        );
    }

    //================================AdditionalMethods===============================
    private async Task SendStateInfoMessage(State state,Chat chat,string text,UserState newState)
    {
        var messageId = (await botClient.SendMessage(
            chat.Id,
            text.FormateString(),
            parseMode: ParseMode.MarkdownV2
            )).Id;
        state.messagesIds.Push(messageId);
        state.StateName = newState;
    }
    
    public async Task SetCommandsForUser(Role role, long userId)
    {
        var commands = new List<BotCommand>();

        if (role != Role.Unknown)
        {
            // Общие команды для всех
            commands.AddRange(new[]
                {
                    new BotCommand { Command = "help", Description = "Помощь" },
                    new BotCommand { Command = "profile", Description = "Мой профиль" }
                });
            if (role != Role.Child){
                // Команды для вожатых
                commands.AddRange(new[]
                {
                    new BotCommand { Command = "promote", Description = "Выдать права вожатого" },
                    new BotCommand { Command = "delete", Description = "Удалить информацию о пользователе" },
                    new BotCommand { Command = "add", Description = "добавить нового пользователя" },
                });
            }
        }
        await botClient.SetMyCommands(
            commands,
            scope: new BotCommandScopeChat { ChatId = userId });
    }

    private async Task CancelState(Chat chat,long userId)
    {
        while(stateDict[userId].messagesIds.Count!=0)
            await botClient.DeleteMessage(chat, stateDict[userId].messagesIds.Pop());
        stateDict.Remove(userId);
    }
}

public static class Extention
{
    public static ChildDto ToChildDto(this UserDomain user)
    {
        if (user.ChildInfo == null) throw new ArgumentException();
        return new ChildDto
        {
            Id = user.Id,
            FullName = user.FullName,
            TelegramInfo = user.TelegramInfo,
            Birthday = user.Birthday,
            City = user.City,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            ChildInfo = user.ChildInfo
        };
    }

    public static CounsellorDto ToCounsellorDto(this UserDomain user)
    {
        if (user.CounsellorInfo == null) throw new ArgumentException();
        return new CounsellorDto
        {
            Id = user.Id,
            FullName = user.FullName,
            TelegramInfo = user.TelegramInfo,
            Birthday = user.Birthday,
            City = user.City,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            CounsellorInfo = user.CounsellorInfo
        };
    }
}