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
using OzshBot.Application.Services;
namespace OzshBot.Bot;

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

    public BotHandler(
        ITelegramBotClient botClient,
        ReceiverOptions receiverOptions,
        UserService userService)
    {
        this.botClient = botClient;
        this.receiverOptions = receiverOptions;
        this.userService = userService;
    }

    public async Task SetCommandsForUser(UserTg user)
    {
        var commands = new List<BotCommand>();
        var role = await userService.RoleService.GetUserRole(
            new TelegramInfo { TgUsername = user.Username, TgId = user.Id });

        if (role != Role.Unknown)
        {
            // Общие команды для всех
            commands.AddRange(new[]
                {
                    new BotCommand { Command = "help", Description = "Помощь" },
                    new BotCommand { Command = "profile", Description = "Мой профиль" }
                });
            if (role != Role.Child)
            {
                // Команды для вожатых и админов
                commands.AddRange(new[]
                {
                    new BotCommand { Command = "promote", Description = "Выдать права вожатого" },
                    new BotCommand { Command = "demote", Description = "Забрать права вожатого" }
                });
            }
        }

        await botClient.SetMyCommands(
            commands,
            scope: new BotCommandScopeChat { ChatId = user.Id });
    }

    private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message is not { } message) return;
                    if (message.Text is not { } messageText) return;

                    var role = userService.RoleService.GetUserRole(
                        new TelegramInfo { TgUsername = message.From.Username, TgId = message.From.Id }).Result;
                    var chat = message.Chat;

                    Console.WriteLine($"id: {message.From.Id}\nusername {message.From.Username}\nроль: {role}");



                    //часть для которой не нужны права ======================================================================
                    if (messageText == "/start")
                    {
                        await SetCommandsForUser(message.From);
                        return;
                    }
                    //=======================================================================================================

                    //если нет прав, то отвергаю все остальное ==============================================================
                    if (role == Role.Unknown) return;

                    if (messageText.StartsWith("/") == false)
                    {
                        var result = await userService.FindService.FindUserAsync(messageText);

                        switch (result)
                        {
                            case {IsSuccess:true}:
                                var users = result.Value;
                                var children = users.GetChildren();
                                var counsellors = users.GetCounsellors();
                                if (users.Count() == 1)
                                    await botClient.SendMessage(
                                        chat.Id,
                                        FormDataAnswer(users.First(), role),
                                        parseMode: ParseMode.MarkdownV2
                                        );
                                else
                                    await botClient.SendMessage(
                                        chat.Id,
                                        FormManyPeopleDataAnswer(children.ToArray(), counsellors.ToArray()),
                                        parseMode: ParseMode.MarkdownV2
                                        );
                                return;

                            case{IsFailed:true}:
                                await botClient.SendMessage(
                                    chat.Id,
                                    $"никто не найден",
                                    parseMode: ParseMode.MarkdownV2
                                    );
                                return;
                        }
                    }
                    if (messageText == "/profile")
                    {
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
                        return;
                    }
                    //=======================================================================================================

                    //если нет прав писать(давать права и тд), то отвергаю все остальное ====================================
                    if (role == Role.Child) return;
                    if (messageText.StartsWith("/promote"))
                        await userService.RoleService.PromoteToCounsellor(
                            new TelegramInfo { TgUsername = messageText.Split()[1] });
                    //=======================================================================================================

                    break;

                case UpdateType.InlineQuery:
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
    

    private string FormDataAnswer(UserDomain user, Role role)
    {
        var answer = "";
        if(user.Role == Role.Child)
        {
            var childInfo = user.ChildInfo;
            answer +=
                $"{childInfo.FullName}\n" +
                $"@{user.TelegramInfo.TgUsername}\n" +
                $"Группа {childInfo.Group}\n" +
                $"Город: `{childInfo.City}`\n" +
                $"Школа: `{childInfo.EducationInfo.School}`, {childInfo.EducationInfo.Class} класс\n\n" +
                $"Дата рождения: {childInfo.Birthday}";

            if (role == Role.Counsellor || role == Role.Developer)
            {
                answer += "\n\n" +
                    $"Почта: `{childInfo.Email}`\n" +
                    $"Телефон: `{childInfo.PhoneNumber}`";

                if (childInfo.Parents.Length != 0)
                    answer += "\n\n" +
                        "Родители:\n" +
                        String.Join("\n", childInfo.Parents
                            .Select(parent =>
                                $" -{parent.FullName}\n" +
                                $"   `{parent.PhoneNumber}`"));
            }
        }
        else if (user.Role == Role.Counsellor)
        {
            var counsellorInfo = user.CounsellorInfo;
            answer +=
                $"{counsellorInfo.FullName}\n" +
                $"@{user.TelegramInfo.TgUsername}\n" +
                $"Группа {counsellorInfo.Group}\n" +
                $"Город: `{counsellorInfo.City}`\n\n" +
                $"Дата рождения: {counsellorInfo.Birthday}";

            if (role == Role.Counsellor || role == Role.Developer)
                answer += "\n\n" +
                    $"Почта: `{counsellorInfo.Email}`\n" +
                    $"Телефон: `{counsellorInfo.PhoneNumber}`";

        }
        return answer.Replace(".","\\.").Replace("-","\\-");
    }

    private string FormManyPeopleDataAnswer(ChildDto[] children, CounsellorDto[] counsellors)
    {
        var answer = "";
        if (children.Length != 0)
            answer += "Дети:\n" +
                String.Join("\n", children
                        .Select(child =>
                            $" -`{child.ChildInfo.FullName}` @{child.TelegramInfo.TgUsername} группа {child.ChildInfo.Group}"
                        ))+"\n\n";
        if (counsellors.Length != 0)
            answer += "Вожатые:\n" +
                String.Join("\n", counsellors
                        .Select(counsellor =>
                            $" -`{counsellor.CounsellorInfo.FullName}` @{counsellor.TelegramInfo.TgUsername} группа {counsellor.CounsellorInfo.Group}"
                        ));
        return answer.Replace(".","\\.").Replace("-","\\-");
    }

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
        container.Bind<ReceiverOptions>().ToConstant(new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message } });
        container.Bind<UserService>().ToSelf();
        container.Bind<IUserManagementService>().To<UserManagementService>();
        container.Bind<IUserRoleService>().To<MyUserRoleService>();
        container.Bind<IUserFindService>().To<UserFindService>();
        container.Bind<IUserRepository>().To<MyUserRepository>();
        container.Bind<MadeUpData>().ToConstant(new MadeUpData());

        return container;
    }
}

static class Extention
{
    public static IEnumerable<ChildDto> GetChildren(this IEnumerable<UserDomain> users)
    {
        return users.Where(user => user.Role==Role.Child).Select(user=> new ChildDto
        {
            TelegramInfo = user.TelegramInfo,
            ChildInfo = user.ChildInfo
        });
    }

    public static IEnumerable<CounsellorDto> GetCounsellors(this IEnumerable<UserDomain> users)
    {
        return users.Where(user => user.Role==Role.Counsellor).Select(user=> new CounsellorDto
        {
            TelegramInfo = user.TelegramInfo,
            CounsellorInfo = user.CounsellorInfo
        });
    }
}