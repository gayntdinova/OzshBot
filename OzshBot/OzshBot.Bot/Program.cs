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
        this.EditService = editService;
        this.AccessRightsService = accessRightsService;
        this.FindService = findService;
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

        var role = userService.AccessRightsService.GetAccessRightsAsync(
            new TelegramInfo { TgUsername = user.Username, TgId = user.Id });

        if (role.Result == AccessRights.Write)
        {
            // Команды для вожатых и админов
            commands.AddRange(new[]
            {
                new BotCommand { Command = "promote", Description = "Выдать права вожатого" },
                new BotCommand { Command = "demote", Description = "Забрать права вожатого" },
                new BotCommand { Command = "list", Description = "Список вожатых" }
            });
        }

        if (role.Result == AccessRights.Read)
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
                    var role = userService.AccessRightsService.GetAccessRightsAsync(
                        new TelegramInfo { TgUsername = message.From.Username, TgId = message.From.Id }).Result;

                    var chat = message.Chat;

                    Console.WriteLine($"id: {message.From.Id}\nusername {message.From.Username}\nроль: {role}");

                    //старт
                    if (messageText == "/start")
                    {
                        await SetCommandsForUser(message.From);
                        return;
                    }

                    //если нет прав, то отвергаю все остальное
                    if (role == AccessRights.NoRights) 
                        return;

                    if (messageText.StartsWith("/") == false)
                    {
                        //todo-----------------------------------------------------------
                        await botClient.SendMessage(
                            chat.Id,
                            $"какая то информация"
                            );
                            return;
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

                    //если нет прав писать(давать права и тд), то отвергаю все остальное
                    if (role == AccessRights.Read)
                        return;

                    if (messageText.StartsWith("/promote"))
                    {
                        var userToPromote = messageText.Split()[1];
                        await userService.AccessRightsService.PromoteToCounsellor(
                            new TelegramInfo { TgUsername = userToPromote });
                        return;
                    }
                    
                    if(messageText.StartsWith("/demote"))
                    {
                        var userToPromote = messageText.Split()[1];
                        await userService.AccessRightsService.DemoteAccessRightsAsync(
                            new TelegramInfo { TgUsername = userToPromote });
                        return;
                    }
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

        return container;
    }
}
