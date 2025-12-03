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
namespace OzshBot.Bot;


public enum UserState
{
    // CreatingUser_Role,
    // CreatingUser_WaitingFullName,
    // CreatingUser_WaitingTgUsername,
    // CreatingUser_WaitingBirthday,
    // CreatingUser_WaitingCity,
    // CreatingUser_WaitingPhoneNumber,
    // CreatingUser_WaitingEmail,
    // CreatingUser_WaitingClass,
    // CreatingUser_WaitingSchool,


    EditingUser_SelectField,

    EditingUser_WaitingFullName,
    EditingUser_WaitingTgUsername,
    EditingUser_WaitingBirthday,
    EditingUser_WaitingCity,
    EditingUser_WaitingPhoneNumber,
    EditingUser_WaitingEmail,
    EditingUser_WaitingClass,
    EditingUser_WaitingSchool,
    EditingUser_WaitingGroup
}

public class State
{
    public UserState StateName;
    public UserDomain Data;
    public Stack<MessageId> messagesIds = new();

    public State(UserState name, UserDomain? user = null)
    {
        StateName = name;
        if (user == null)
            Data = new UserDomain{FullName = new(),TelegramInfo = new TelegramInfo{TgUsername = ""}};
        else
            Data = user;
    }
}

public class UserService
{
    public IUserManagementService ManagementService;
    public IUserRoleService RoleService;
    public IUserFindService FindService;

    public UserService(
        IUserManagementService managementService,
        IUserRoleService roleService,
        IUserFindService findService)
    {
        ManagementService = managementService;
        RoleService = roleService;
        FindService = findService;
    }
}

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

    public async Task SetCommandsForUser(string username, long userId)
    {
        var commands = new List<BotCommand>();
        var role = await userService.RoleService.GetUserRole(
            new TelegramInfo { TgUsername = username, TgId = userId });

        if (role != Role.Unknown)
        {
            // Общие команды для всех
            commands.AddRange(new[]
                {
                    new BotCommand { Command = "help", Description = "Помощь" },
                    new BotCommand { Command = "profile", Description = "Мой профиль" }
                });
            if (role != Role.Child){
                // Команды для вожатых и админов
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

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    HandleMessage(update);
                    break;

                case UpdateType.CallbackQuery:
                    HandleCallbackQuery(update);
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

    private async void HandleMessage(Update update)
    {
        if (update.Message is not { } message) return;
        if (message.Text is not { } messageText) return;
        if (message.From == null) return;//хз
        if (message.From.Username == null) return;//хз
        var splittedMessage = messageText.Split();

        var role = userService.RoleService.GetUserRole(
            new TelegramInfo { TgUsername = message.From.Username, TgId = message.From.Id }).Result;
        var chat = message.Chat;

        Console.WriteLine($"id: {message.From.Id}\nusername {message.From.Username}\nроль: {role}");

        if (stateDict.Keys.Contains(chat.Id))
        {
            var state = stateDict[chat.Id];

            switch (state.StateName)
            {
                case UserState.EditingUser_SelectField:
                    CancelEditing(chat.Id,chat);
                    break;

                case UserState.EditingUser_WaitingFullName:
                    state.messagesIds.Push(message.Id);
                    if (Regex.IsMatch(messageText, @"^[А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+ [А-ЯЁ][а-яё]+$"))
                    {
                        state.StateName = UserState.EditingUser_SelectField;
                        state.Data.FullName.Surname = splittedMessage[0];
                        state.Data.FullName.Name = splittedMessage[1];
                        state.Data.FullName.Patronymic = splittedMessage[2];
                    }
                    else
                        SendEditInfoMessage(state,chat,
                            "Некорректный формат\nВведите новое имя полностью (корректный формат: Фамилия Имя Отчество)",
                            UserState.EditingUser_WaitingBirthday);
                    return;

                case UserState.EditingUser_WaitingTgUsername:
                    state.messagesIds.Push(message.Id);
                    if (Regex.IsMatch(messageText, @"^@[A-Za-z0-9_]+$"))
                    {
                        state.StateName = UserState.EditingUser_SelectField;
                        state.Data.TelegramInfo.TgUsername = messageText.Substring(1);
                    }
                    else
                        SendEditInfoMessage(state,chat,
                            "Некорректный формат\nВведите новый юзернейм телеграма(корректный формат: @username)",
                            UserState.EditingUser_WaitingTgUsername);
                    return;

                case UserState.EditingUser_WaitingBirthday:
                    state.messagesIds.Push(message.Id);
                    if (Regex.IsMatch(messageText, @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                    {
                        state.StateName = UserState.EditingUser_SelectField;
                        state.Data.Birthday = DateOnly.ParseExact(messageText, "dd.MM.yyyy");
                    }
                    else
                        SendEditInfoMessage(state,chat,
                            "Некорректный формат\nВведите новый день рождения(корректный формат: день.месяц.год)",
                            UserState.EditingUser_WaitingBirthday);
                    return;

                case UserState.EditingUser_WaitingCity:
                    state.messagesIds.Push(message.Id);

                    state.StateName = UserState.EditingUser_SelectField;
                    state.Data.City = messageText;
                    return;

                case UserState.EditingUser_WaitingPhoneNumber:
                    state.messagesIds.Push(message.Id);
                    if (Regex.IsMatch(messageText, @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"))
                    {
                        state.StateName = UserState.EditingUser_SelectField;
                        state.Data.PhoneNumber = messageText;
                    }
                    else
                        SendEditInfoMessage(state,chat,
                            "Некорректный формат\nВведите новый номер телефона(корректный формат: +7**********)",
                            UserState.EditingUser_WaitingPhoneNumber);
                    return;
                    
                case UserState.EditingUser_WaitingEmail:
                    state.messagesIds.Push(message.Id);
                    if (Regex.IsMatch(messageText, @"^[\w\.\-]+@[\w\-]+\.[A-Za-z]{2,}$"))
                    {
                        state.StateName = UserState.EditingUser_SelectField;
                        state.Data.Email = messageText;
                    }
                    else
                        SendEditInfoMessage(state,chat,
                            "Некорректный формат\nВведите новый email(корректный формат: чтото@чтото.чтото)",
                            UserState.EditingUser_WaitingEmail);
                    return;

                case UserState.EditingUser_WaitingClass:
                    state.messagesIds.Push(message.Id);
                    if (Regex.IsMatch(messageText, @"^(?:[1-9]|10|11)$"))
                    {
                        state.StateName = UserState.EditingUser_SelectField;
                        if (state.Data.ChildInfo!=null)
                            state.Data.ChildInfo.EducationInfo.Class =int.Parse(messageText);
                    }
                    else
                        SendEditInfoMessage(state,chat,
                            "Некорректный формат\nВведите новый номер класса(год обучения)",
                            UserState.EditingUser_WaitingClass);
                    return;

                case UserState.EditingUser_WaitingSchool:
                    state.messagesIds.Push(message.Id);

                    state.StateName = UserState.EditingUser_SelectField;
                    if (state.Data.ChildInfo!=null)
                        state.Data.ChildInfo.EducationInfo.School = messageText;
                    return;

                case UserState.EditingUser_WaitingGroup:
                    state.messagesIds.Push(message.Id);
                    if (Regex.IsMatch(messageText, @"^\d+$"))
                    {
                        state.StateName = UserState.EditingUser_SelectField;
                        if (state.Data.ChildInfo!=null)
                            state.Data.ChildInfo.Group = int.Parse(messageText);
                        if (state.Data.CounsellorInfo!=null)
                            state.Data.CounsellorInfo.Group = int.Parse(messageText);
                    }
                    else
                        SendEditInfoMessage(state,chat,
                            "Некорректный формат\nВведите новый номер группы",
                            UserState.EditingUser_WaitingGroup);
                    return;
            }
        }

        if(role == Role.Unknown)
            await botClient.SendMessage(
                chat.Id,
                "для того чтобы пользоваться этим ботом вы должны быть учавстником лагеря ОЗШ",
                parseMode: ParseMode.MarkdownV2
            );

        switch (splittedMessage[0])
        {
            case "/start":
                await SetCommandsForUser(message.From.Username,message.From.Id);
                break;
            case "/profile":
                var you = await userService.FindService.FindUserByTgAsync(new TelegramInfo{
                    TgUsername = message.From.Username,
                    TgId = null});
                if(you.IsFailed)
                    await botClient.SendMessage(
                        chat.Id,
                        $"вас не существует",
                        parseMode: ParseMode.MarkdownV2
                        );
                else if (you.Value.Role == Role.Child)
                    await botClient.SendMessage(
                        chat.Id,
                        FormDataAnswer(you.Value, role),
                        parseMode: ParseMode.MarkdownV2
                        );
                break;

            case "/promote":
                if (role == Role.Child)
                {
                    await botClient.SendMessage(
                        chat.Id,
                        "у вас нет прав пользоваться этой командой",
                        parseMode: ParseMode.MarkdownV2
                        );
                    break;
                }
                //todo
                break;

            default:
                var result = await userService.FindService.FindUserAsync(messageText);

                switch (result)
                {
                    case {IsSuccess:true}:
                        var users = result.Value;
                        if (users.Count() == 1)
                        {
                            if(role == Role.Counsellor || role == Role.Developer)
                                await botClient.SendMessage(
                                    chat.Id,
                                    FormDataAnswer(users[0], role),
                                    replyMarkup: new InlineKeyboardMarkup(
                                        InlineKeyboardButton.WithCallbackData("Редактировать", "editMenu "+users[0].TelegramInfo.TgUsername)
                                    ),
                                    parseMode: ParseMode.MarkdownV2
                                    );
                            else
                                await botClient.SendMessage(
                                    chat.Id,
                                    FormDataAnswer(users[0], role),
                                    parseMode: ParseMode.MarkdownV2
                                    );
                        }
                        else
                            await botClient.SendMessage(
                                chat.Id,
                                FormManyPeopleDataAnswer(users),
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
                break;
        }
    }

    private async void HandleCallbackQuery(Update update)
    {
        if (update.CallbackQuery is not { } callback) return;
        if (callback.Message == null) return;//хз
        if (callback.Message.From == null) return;//хз
        if (callback.Message.From.Username == null)return;//хз
        if (callback.Data == null) return;//хз
        MessageId messageId;
        var chat = callback.Message.Chat;
        var splittedCommand = callback.Data.Split();

        switch (splittedCommand[0])
        {
            case "editMenu":
                if(stateDict.Keys.Contains(chat.Id))
                    CancelEditing(chat.Id, chat);

                var username = splittedCommand[1];
                var redactedUser = await userService.FindService.FindUserByTgAsync(new TelegramInfo{TgUsername = username});

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
                stateDict[chat.Id] = newState;
                break;

            case "edit":
                var state = stateDict[chat.Id];
                if(stateDict.Keys.Contains(chat.Id) && state.StateName != UserState.EditingUser_SelectField)
                    await botClient.DeleteMessage(chat, state.messagesIds.Pop());

                switch (splittedCommand[1])
                {
                    case "name":
                        SendEditInfoMessage(state,chat,
                            "Введите новое имя полностью (корректный формат: Фамилия Имя Отчество)",
                            UserState.EditingUser_WaitingFullName);
                        break;
                    case "telegram":
                        SendEditInfoMessage(state,chat,
                            "Введите новый юзернейм телеграма(корректный формат: @username)",
                            UserState.EditingUser_WaitingTgUsername);
                        break;
                    case "birth":
                        SendEditInfoMessage(state,chat,
                            "Введите новый день рождения(корректный формат: день.месяц.год)",
                            UserState.EditingUser_WaitingBirthday);
                        break;
                    case "city":
                        SendEditInfoMessage(state,chat,
                            "Введите новое название города",
                            UserState.EditingUser_WaitingCity);
                        break;
                    case "phone":
                        SendEditInfoMessage(state,chat,
                            "Введите новый номер телефона(корректный формат: +7**********)",
                            UserState.EditingUser_WaitingPhoneNumber);
                        break;
                    case "email":
                        SendEditInfoMessage(state,chat,
                            "Введите новый email(корректный формат: чтото@чтото.чтото)",
                            UserState.EditingUser_WaitingEmail);
                        break;
                    case "class":
                        SendEditInfoMessage(state,chat,
                            "Введите новый номер класса(год обучения)",
                            UserState.EditingUser_WaitingClass);
                        break;
                    case "school":
                        SendEditInfoMessage(state,chat,
                            "Введите новое название школы",
                            UserState.EditingUser_WaitingSchool);
                        break;
                    case "group":
                        SendEditInfoMessage(state,chat,
                            "Введите новый номер группы",
                            UserState.EditingUser_WaitingGroup);
                        break;
                    case "cancel":
                        CancelEditing(chat.Id,chat);
                        break;
                    case "apply":
                        await userService.ManagementService.EditUser(new TelegramInfo
                            {
                                TgUsername = callback.Message.From.Username,
                                TgId = callback.Message.From.Id
                            }
                            ,state.Data);
                        CancelEditing(chat.Id,chat);
                        break;
                }
                break;
        }
    }

    private async void SendEditInfoMessage(State state,Chat chat,string text,UserState newState)
    {
        var messageId = (await botClient.SendMessage(
            chat.Id,
            FormatString(text),
            parseMode: ParseMode.MarkdownV2
            )).Id;
        state.messagesIds.Push(messageId);
        state.StateName = newState;
    }
    
    private async void CancelEditing(long id, Chat chat)
    {
        while(stateDict[id].messagesIds.Count!=0)
            await botClient.DeleteMessage(chat, stateDict[id].messagesIds.Pop());
        stateDict.Remove(id);
    }

    private string FormDataAnswer(UserDomain user, Role role)
    {
        var answer = "";
        if(user.Role == Role.Child && user.ChildInfo!= null)//ненужная проверка
        {
            var childInfo = user.ChildInfo;
            answer +=
                $"{user.FullName}\n" +
                $"@{user.TelegramInfo.TgUsername}\n" +
                $"Группа {childInfo.Group}\n" +
                $"Город: `{user.City}`\n" +
                $"Школа: `{childInfo.EducationInfo.School}`, {childInfo.EducationInfo.Class} класс\n\n" +
                $"Дата рождения: {user.Birthday}";

            if (role == Role.Counsellor || role == Role.Developer)
            {
                answer += "\n\n" +
                    $"Почта: `{user.Email}`\n" +
                    $"Телефон: `{user.PhoneNumber}`";

                if (childInfo.ContactPeople.Length != 0)
                    answer += "\n\n" +
                        "Родители:\n" +
                        String.Join("\n", childInfo.ContactPeople
                            .Select(parent =>
                                $" - {parent.FullName}\n" +
                                $"   `{parent.PhoneNumber}`"));
            }
        }
        else if (user.Role == Role.Counsellor && user.CounsellorInfo!=null)//ещё лишняя проверка
        {
            var counsellorInfo = user.CounsellorInfo;
            answer +=
                $"{user.FullName}\n" +
                $"@{user.TelegramInfo.TgUsername}\n" +
                $"Группа {counsellorInfo.Group}\n" +
                $"Город: `{user.City}`\n\n" +
                $"Дата рождения: {user.Birthday}";

            if (role == Role.Counsellor || role == Role.Developer)
                answer += "\n\n" +
                    $"Почта: `{user.Email}`\n" +
                    $"Телефон: `{user.PhoneNumber}`";

        }
        return FormatString(answer);
    }

    private string FormManyPeopleDataAnswer(IEnumerable<UserDomain> users)
    {
        var children = users.Where(user=>user.Role == Role.Child);
        var counsellors = users.Where(user=>user.Role == Role.Counsellor);
        var answer = "";
        if (children.Count() != 0)
            answer += "Дети:\n" +
                String.Join("\n", children
                        .Select(child =>
                            $" - `{child.FullName}` {child.Birthday} @{child.TelegramInfo.TgUsername} группа {child.ChildInfo.Group}"
                        ))+"\n\n";
        if (counsellors.Count() != 0)
            answer += "Вожатые:\n" +
                String.Join("\n", counsellors
                        .Select(counsellor =>
                            $" - `{counsellor.FullName}` {counsellor.Birthday} @{counsellor.TelegramInfo.TgUsername} группа {counsellor.CounsellorInfo.Group}"
                        ));
        return FormatString(answer);
    }

    private string FormatString(string text)
    => text.Replace(".","\\.").Replace("-","\\-").Replace("+","\\+").Replace("*","\\*")
        .Replace("(","\\(").Replace(")","\\)");

    private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        Console.WriteLine(error switch
        {
            _ => error.ToString()
        });

        return Task.CompletedTask;
    }

    public async Task Start()
    {
        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

        Console.WriteLine($"{(await botClient.GetMe()).FirstName} запущен!");

        await Task.Delay(-1);
    }
}

static class Program
{
    static async Task Main()
    {
        var container = ConfigureContainer();
        var botHandler = container.Get<BotHandler>();
        await botHandler.Start();
    }

    public static StandardKernel ConfigureContainer()
    {
        var container = new StandardKernel();

        container.Bind<ITelegramBotClient>().ToConstant(new TelegramBotClient("8445241215:AAE-fg7HdNllMonKukdR5T9e_8I4e4FwpXg"));
        container.Bind<ReceiverOptions>().ToConstant(new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message,UpdateType.CallbackQuery } });
        container.Bind<UserService>().ToSelf();
        container.Bind<IUserManagementService>().To<UserManagementService>();
        container.Bind<ITableParser>().ToConstant(new MyTableParser());
        container.Bind<IUserRoleService>().To<MyUserRoleService>();
        container.Bind<IUserFindService>().To<UserFindService>();
        container.Bind<IUserRepository>().To<MyUserRepository>();
        container.Bind<MadeUpData>().ToConstant(new MadeUpData());

        return container;
    }
}

// static class Extention
// {
//     public static IEnumerable<ChildDto> GetChildren(this IEnumerable<UserDomain> users)
//     {
//         return users.Where(user => user.Role==Role.Child).Select(user=> new ChildDto
//         {
//             TelegramInfo = user.TelegramInfo,
//             ChildInfo = user.ChildInfo
//         });
//     }

//     public static IEnumerable<CounsellorDto> GetCounsellors(this IEnumerable<UserDomain> users)
//     {
//         return users.Where(user => user.Role==Role.Counsellor).Select(user=> new CounsellorDto
//         {
//             TelegramInfo = user.TelegramInfo,
//             CounsellorInfo = user.CounsellorInfo
//         });
//     }
// }