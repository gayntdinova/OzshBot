using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System;
using OzshBot.Application.Interfaces;
using OzshBot.Application.Implementations;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Ninject;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Formats.Asn1;
namespace OzshBot.Bot;

public class UserService
{
    public IEditService EditService;
    public IAccessRightsService AccessRightsService;
    public IFindService FindService;

    public UserService(
        IEditService editService,
        IAccessRightsService accessRightsService,
        IFindService findService)
    {
        EditService = editService;
        AccessRightsService = accessRightsService;
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

    public async Task SetCommandsForUser(User user)
    {
        var commands = new List<BotCommand>();

        var rights = userService.AccessRightsService.GetAccessRightsAsync(
            new TelegramInfo { TgUsername = user.Username, TgId = user.Id });

        if (rights.Result == AccessRights.Write)
        {
            // Команды для вожатых и админов
            commands.AddRange(new[]
            {
                new BotCommand { Command = "promote", Description = "Выдать права вожатого" },
                new BotCommand { Command = "demote", Description = "Забрать права вожатого" },
                new BotCommand { Command = "list", Description = "Список вожатых" }
            });
        }

        if (rights.Result == AccessRights.Read)
        {
            // Общие команды для всех
            commands.AddRange(new[]
            {
                new BotCommand { Command = "help", Description = "Помощь" },
                new BotCommand { Command = "profile", Description = "Мой профиль" }
            });
        }

        try
        {
            await botClient.SetMyCommands(
                commands,
                scope: new BotCommandScopeChat { ChatId = user.Id });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка установки команд: {ex.Message}");
        }
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

                    var rights = userService.AccessRightsService.GetAccessRightsAsync(
                        new TelegramInfo { TgUsername = message.From.Username, TgId = message.From.Id }).Result;
                    var chat = message.Chat;

                    Console.WriteLine($"id: {message.From.Id}\nusername {message.From.Username}\nроль: {rights}");

                    //часть для которой не нужны права ======================================================================


                    if (messageText == "/start")
                    {
                        await SetCommandsForUser(message.From);
                        return;
                    }


                    //если нет прав, то отвергаю все остальное ==============================================================
                    if (rights == AccessRights.NoRights) return;

                    if (messageText.StartsWith("/") == false)
                    {
                        (var children, var counsellors) = FindUsersAsync(messageText).Result;

                        if (counsellors.Length == 0 && counsellors.Length == 0)
                            await botClient.SendMessage(
                                chat.Id,
                                $"никто не найден"
                                );
                        else if (counsellors.Length == 0 && children.Length == 1)
                            await botClient.SendMessage(
                                chat.Id,
                                FormChildDataAnswer(children[0], rights)
                                );
                        else if (counsellors.Length == 1 && children.Length == 0)
                            await botClient.SendMessage(
                                chat.Id,
                                FormCounsellorDataAnswer(counsellors[0], rights)
                                );
                        else
                            await botClient.SendMessage(
                                chat.Id,
                                FormManyPeopleDataAnswer(children, counsellors)
                                );
                    }
                    if (messageText == "/profile")
                    {
                        //todo------------------------------------------------------------
                        await botClient.SendMessage(
                            chat.Id,
                            $"профиль"
                            );
                        return;
                    }


                    //если нет прав писать(давать права и тд), то отвергаю все остальное ====================================
                    if (rights == AccessRights.Read) return;

                    if (messageText.StartsWith("/promote"))
                        await userService.AccessRightsService.PromoteToCounsellor(
                            new TelegramInfo { TgUsername = messageText.Split()[1] });

                    if (messageText.StartsWith("/demote"))
                        await userService.AccessRightsService.DemoteAccessRightsAsync(
                            new TelegramInfo { TgUsername = messageText.Split()[1] });
                    return;

                case UpdateType.InlineQuery:
                    return;
                default:
                    return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private string FormChildDataAnswer(Child child, AccessRights rights)
    {
        var answer =
            $"{child.FullName.Name} {child.FullName.Surname} {child.FullName.Surname}\n" +
            $"@{child.TelegramInfo.TgUsername}" +
            $"Группа {child.Group}\n" +
            $"Город: `{child.Town}`\n" +
            $"Школа: `{child.EducationInfo.School}`, {child.EducationInfo.Class} класс\n\n" +
            $"Дата рождения: {child.Birthday}";

        if (rights == AccessRights.Write)
        {
            answer += "\n\n" +
                $"Почта: `{child.Email}`\n" +
                $"Телефон: `{child.PhoneNumber}`";

            if (child.Parents != null)
                answer += "\n\n" +
                    "Родители:\n" +
                    String.Join("\n", child.Parents
                        .Select(parent =>
                            $" - {parent.FullName.Name} {parent.FullName.Surname} {parent.FullName.Patronymic}\n" +
                            $"   `{parent.PhoneNumber}`"));
        }
        return answer;
    }

    private string FormCounsellorDataAnswer(Counsellor counsellor, AccessRights rights)
    {
        var answer =
            $"{counsellor.FullName.Name} {counsellor.FullName.Surname} {counsellor.FullName.Surname}\n" +
            $"@{counsellor.TelegramInfo.TgUsername}" +
            $"Группа {counsellor.Group}\n" +
            $"Город: `{counsellor.Town}`\n\n" +
            $"Дата рождения: {counsellor.Birthday}";

        if (rights == AccessRights.Write)
            answer += "\n\n" +
                $"Почта: `{counsellor.Email}`\n" +
                $"Телефон: `{counsellor.PhoneNumber}`";

        return answer;
    }

    private string FormManyPeopleDataAnswer(Child[] children, Counsellor[] counsellors)
    {
        var answer = "";
        if (children.Length != 0)
            answer += "Дети:\n" +
                String.Join("\n", children
                        .Select(child =>
                            $" - `{child.FullName.Name} {child.FullName.Surname} {child.FullName.Patronymic}` {child.Birthday} {child.TelegramInfo.TgUsername} группа {child.Group}"
                        ));
        if (counsellors.Length != 0)
            answer += "Вожатые:\n" +
                String.Join("\n", counsellors
                        .Select(counsellor =>
                            $" - `{counsellor.FullName.Name} {counsellor.FullName.Surname} {counsellor.FullName.Patronymic}` {counsellor.Birthday} {counsellor.TelegramInfo.TgUsername} группа {counsellor.Group}"
                        ));
        return answer;
    }

    private async Task<(Child[], Counsellor[])> FindUsersAsync(string message)
    {
        var splittedMessage = message.Split(" ");
        var children = new List<Child>();
        var counsellors = new List<Counsellor>();

        if (message.StartsWith("@"))
        {
            await AddUsesByUsername(children, counsellors, message.Substring(1));
        }
        else if (splittedMessage.Length == 1)
        {
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[0], null, null);
            await AddUsersByPartOfFullName(children, counsellors, null, splittedMessage[0], null);
            await AddUsersByPartOfFullName(children, counsellors, null, null, splittedMessage[0]);
        }
        else if (splittedMessage.Length == 2)
        {
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[0], splittedMessage[1], null);
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[1], splittedMessage[0], null);
            await AddUsersByPartOfFullName(children, counsellors, null, splittedMessage[0], splittedMessage[1]);
            await AddUsersByPartOfFullName(children, counsellors, null, splittedMessage[1], splittedMessage[0]);
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[0], null, splittedMessage[1]);
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[1], null, splittedMessage[0]);
        }
        else if (splittedMessage.Length == 3)
        {
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[0], splittedMessage[1], splittedMessage[2]);
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[0], splittedMessage[2], splittedMessage[1]);
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[1], splittedMessage[0], splittedMessage[2]);
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[1], splittedMessage[2], splittedMessage[0]);
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[2], splittedMessage[0], splittedMessage[1]);
            await AddUsersByPartOfFullName(children, counsellors, splittedMessage[2], splittedMessage[1], splittedMessage[0]);
        }

        await AddUsersByTown(children, counsellors, message);
        await AddUsersByClass(children, counsellors, message);
        await AddUsersByGroup(children, counsellors, message);

        return (children.ToArray(), counsellors.ToArray());
    }

    private async Task AddUsesByUsername(List<Child> children, List<Counsellor> counsellors, string username)
    {
        var dts = userService.FindService.FindUserByTgUserNameAsync(username).Result;
        if (dts.Child != null)
            children.Add(dts.Child);
        if (dts.Counsellor != null)
            counsellors.Add(dts.Counsellor);
    }

    private async Task AddUsersByPartOfFullName(List<Child> children, List<Counsellor> counsellors, string? name, string? surname, string? patronymic)
    {
        var usersByPartOfFullName = userService.FindService.FindUsersByFullNameAsync(
            new FullName { Name = name, Surname = surname, Patronymic = patronymic });
        if (usersByPartOfFullName.Result.Child != null)
            children.AddRange(usersByPartOfFullName.Result.Child);
        if (usersByPartOfFullName.Result.Counsellor != null)
            counsellors.AddRange(usersByPartOfFullName.Result.Counsellor);
    }

    private async Task AddUsersByTown(List<Child> children, List<Counsellor> counsellors, string town)
    {
        var usersByPartOfFullName = userService.FindService.FindUsersByTownAsync(town);
        if (usersByPartOfFullName.Result.Child != null)
            children.AddRange(usersByPartOfFullName.Result.Child);
        if (usersByPartOfFullName.Result.Counsellor != null)
            counsellors.AddRange(usersByPartOfFullName.Result.Counsellor);
    }

    private async Task AddUsersByClass(List<Child> children, List<Counsellor> counsellors, string className)
    {
        var usersByPartOfFullName = userService.FindService.FindUsersByClassAsync(int.Parse(className));
        if (usersByPartOfFullName.Result.Child != null)
            children.AddRange(usersByPartOfFullName.Result.Child);
        if (usersByPartOfFullName.Result.Counsellor != null)
            counsellors.AddRange(usersByPartOfFullName.Result.Counsellor);
    }

    private async Task AddUsersByGroup(List<Child> children, List<Counsellor> counsellors, string group)
    {
        var usersByPartOfFullName = userService.FindService.FindUsersByGroupAsync(int.Parse(group));
        if (usersByPartOfFullName.Result.Child != null)
            children.AddRange(usersByPartOfFullName.Result.Child);
        if (usersByPartOfFullName.Result.Counsellor != null)
            counsellors.AddRange(usersByPartOfFullName.Result.Counsellor);
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

        container.Bind<TelegramBotClient>().ToConstant(new TelegramBotClient("8445241215:AAE-fg7HdNllMonKukdR5T9e_8I4e4FwpXg"));
        container.Bind<ReceiverOptions>().ToConstant(new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message } });
        container.Bind<IFindService>().To<MyFindServise>();
        container.Bind<IEditService>().To<MyEditServise>();
        container.Bind<MadeUpData>().ToConstant(new MadeUpData());

        return container;
    }
}
