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
    
    //нужно для регистрации пользователей
    private IUserRepository userRepository;

    public BotHandler(
        ITelegramBotClient botClient,
        ReceiverOptions receiverOptions,
        UserService userService,
        IUserRepository userRepository)
    {
        this.botClient = botClient;
        this.receiverOptions = receiverOptions;
        this.userService = userService;
        this.userRepository = userRepository;
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
        if (message.From == null) return;//хз
        if (message.From.Username == null) return;//хз

        var role = await HandleMessageIfRegestration(message);
        
        if (message.Text is not { } messageText) return;
        var splittedMessage = messageText.Split();
        var chat = message.Chat;

        Console.WriteLine($"id: {message.From.Id}\nusername {message.From.Username}\nроль: {role}"+(stateDict.Keys.Contains(message.From.Id)?("\nсостояние: "+ stateDict[message.From.Id].StateName):""));

        //==================================================================================================

        if(role == Role.Unknown)
        {
            return;
        }

        if (stateDict.Keys.Contains(message.From.Id))
        {
            await HandleMessageIfState(chat,role,message,message.From.Id);
            return;
        }

        switch (splittedMessage[0])
        {
            case "/start":
                await SetCommandsForUser(role,message.From.Username,message.From.Id);
                break;

            case "/profile":
                await HandleProfile(chat,role,message.From.Username);
                break;

            case "/promote":
                await HandlePromote(chat,role,messageText);
                break;

            default:
                await HandleUsersSearching(chat,role,messageText);
                break;
        }
    }

    private async Task<Role> HandleMessageIfRegestration(Message message)
    {
        var role = userService.RoleService.GetUserRoleByTgAsync(
            new TelegramInfo { TgUsername = message.From.Username, TgId = message.From.Id }).Result;
        if (role != Role.Unknown) return role;
        
        if (!stateDict.Keys.Contains(message.From.Id))
        {
            stateDict[message.From.Id] = new State(UserState.CreatingUser_WaitingForRegistration);
            await SendRegistrationMessage(message);
        }
        else if (stateDict[message.From.Id].StateName == UserState.CreatingUser_WaitingForRegistration)
        {
            var userRegistrator = new UserRegistrator(userRepository);
            role = userRegistrator.LogInAndRegisterUserAsync(message).Result;
            var answer = "для того чтобы пользоваться этим ботом вы должны быть участником лагеря ОЗШ";
            if (role != Role.Unknown)
                answer = "регистрация завершена успешно";
            await botClient.SendMessage(message.Chat.Id, answer, parseMode: ParseMode.MarkdownV2);
            stateDict.Remove(message.From.Id);
        }
        return role;
    }
    
    
    private async Task SendRegistrationMessage(Message message)
    {
        if (message.Text != null)
        {
            Console.WriteLine("AAAAAAAAAAAAAAAA");
            var button = new KeyboardButton("Отправить контакт") { RequestContact = true };

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { button }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        
            await botClient.SendMessage(
                message.Chat.Id,
                "Пожалуйста, отправьте ваш контакт:",
                replyMarkup: keyboard
            );
        }
    }

    private async Task HandleMessageIfState(Chat chat, Role role, Message message,long userId)
    {
        if (message.Text is not { } messageText) return;

        var state = stateDict[userId];

        switch (state.StateName)
        {
            case UserState.EditingUser_SelectField:
                await CancelEditing(userId,chat);
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
                    await SendEditInfoMessage(state,chat,
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
                    await SendEditInfoMessage(state,chat,
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
                    state.numberOfDeletable = 0;
                    state.numberOfDeletable+=2;
                    await SendEditInfoMessage(state,chat,
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
                    await SendEditInfoMessage(state,chat,
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
                    await SendEditInfoMessage(state,chat,
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
                    await SendEditInfoMessage(state,chat,
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
                    await SendEditInfoMessage(state,chat,
                        "Некорректный формат\nВведите новый номер группы",
                        UserState.EditingUser_WaitingGroup);
                }
                return;
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

    private async Task HandlePromote(Chat chat, Role role, string phoneNumber)
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
        if (!Regex.IsMatch(phoneNumber, @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"))
        {
            await botClient.SendMessage(
                chat.Id,
                "Введённая строка не является номером телефона",
                parseMode: ParseMode.MarkdownV2
                );
            return;
        }

        var result = userService.RoleService.PromoteToCounsellorAsync(phoneNumber);
        if (result.IsFaulted)
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
                                users.FormateAnswer(),
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
        MessageId messageId;
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
                if(stateDict.Keys.Contains(userId))
                    await CancelEditing(userId, chat);

                var phoneNumber = splittedCommand[1];
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
                    messageId = (await botClient.SendMessage(
                    chat.Id,
                    "Выбирите что редактировать, если вы напишете сообщение не по теме редактирование отменится",
                    replyMarkup: new InlineKeyboardMarkup(
                        new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Имя", "edit name"),
                                InlineKeyboardButton.WithCallbackData("Телеграм", "edit telegram")
                            },
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("День Рождения", "edit birth"),
                                InlineKeyboardButton.WithCallbackData("Город", "edit city")
                            },
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Телефон", "edit phone"),
                                InlineKeyboardButton.WithCallbackData("Email", "edit email")
                            }
                        }.Concat(redactedUser.Value.Role==Role.Child?
                        new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Класс", "edit class"),
                                InlineKeyboardButton.WithCallbackData("Школа", "edit school"),
                            }
                        }:new InlineKeyboardButton[0][]).Concat(
                        new[]
                        {
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Группа", "edit group"),
                            },
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Отмена", "edit cancel")
                            },
                            new []
                            {
                                InlineKeyboardButton.WithCallbackData("Применить", "edit apply")
                            }

                        }).ToArray()
                        ),
                        parseMode: ParseMode.MarkdownV2
                    )).Id;
                    var newState = new State(UserState.EditingUser_SelectField, redactedUser.Value);
                    newState.messagesIds.Push(messageId);
                    stateDict[userId] = newState;
                }
                break;

            case "edit":
                var state = stateDict[userId];
                if(stateDict.Keys.Contains(userId) && state.StateName != UserState.EditingUser_SelectField)
                    while(state.numberOfDeletable > 0)
                    {
                        await botClient.DeleteMessage(chat, state.messagesIds.Pop());
                        state.numberOfDeletable-=1;
                    }

                switch (splittedCommand[1])
                {
                    case "name":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новое имя полностью (корректный формат: Фамилия Имя Отчество)",
                            UserState.EditingUser_WaitingFullName);
                        break;
                    case "telegram":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новый юзернейм телеграма(корректный формат: @username)",
                            UserState.EditingUser_WaitingTgUsername);
                        break;
                    case "birth":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новый день рождения(корректный формат: день.месяц.год) или введите _ если хотите чтобы этой информации не было",
                            UserState.EditingUser_WaitingBirthday);
                        break;
                    case "city":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новое название города или введите _ если хотите чтобы этой информации не было",
                            UserState.EditingUser_WaitingCity);
                        break;
                    case "phone":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новый номер телефона(корректный формат: +7**********)",
                            UserState.EditingUser_WaitingPhoneNumber);
                        break;
                    case "email":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новый email(корректный формат: чтото@чтото.чтото) или введите _ если хотите чтобы этой информации не было",
                            UserState.EditingUser_WaitingEmail);
                        break;
                    case "class":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новый номер класса(год обучения)",
                            UserState.EditingUser_WaitingClass);
                        break;
                    case "school":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новое название школы",
                            UserState.EditingUser_WaitingSchool);
                        break;
                    case "group":
                        state.numberOfDeletable+=1;
                        await SendEditInfoMessage(state,chat,
                            "Введите новый номер группы или введите _ если хотите чтобы этой информации не было",
                            UserState.EditingUser_WaitingGroup);
                        break;
                    case "cancel":
                        await CancelEditing(userId,chat);
                        break;
                    case "apply":
                        await userService.ManagementService.EditUserAsync(state.Data);
                        await CancelEditing(userId,chat);
                        await botClient.SendMessage(
                            chat.Id,
                            state.Data.FormateAnswer(role),
                            parseMode: ParseMode.MarkdownV2
                            );
                        break;
                }
                break;
        }

        await botClient.AnswerCallbackQuery(callback.Id);
    }

    //================================AdditionalMethods===============================
    private async Task SendEditInfoMessage(State state,Chat chat,string text,UserState newState)
    {
        var messageId = (await botClient.SendMessage(
            chat.Id,
            text.FormateString(),
            parseMode: ParseMode.MarkdownV2
            )).Id;
        state.messagesIds.Push(messageId);
        state.StateName = newState;
    }
    
    public async Task SetCommandsForUser(Role role, string username, long userId)
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
                    new BotCommand { Command = "demote", Description = "Забрать права вожатого" },
                    new BotCommand { Command = "list", Description = "Список вожатых" }
                });
            }
        }
        await botClient.SetMyCommands(
            commands,
            scope: new BotCommandScopeChat { ChatId = userId });
    }

    private async Task CancelEditing(long id, Chat chat)
    {
        while(stateDict[id].messagesIds.Count!=0)
            await botClient.DeleteMessage(chat, stateDict[id].messagesIds.Pop());
        stateDict.Remove(id);
    }
}
