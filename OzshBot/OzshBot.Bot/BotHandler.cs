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
using Ninject.Planning.Bindings.Resolvers;
namespace OzshBot.Bot;

using IBLogger = OzshBot.Application.ToolsInterfaces.ILogger;

public class BotHandler
{
    //клиент для работы с Bot API, позволяет отправлять сообщения, управлять ботом, подписываться на обновления и тд
    public readonly ITelegramBotClient botClient;

    //объект с настройками бота, здесь указываем какие типы Update будем получать, Timeout бота и тд
    private readonly ReceiverOptions receiverOptions;

    //сервис для работы с правами
    public readonly ServiceManager serviceManager;

    private readonly UserRegistrator userRegistrator;

    //список в каком состоянии какие человек
    private Dictionary<long,string> stateDict = new();

    //список комманд
    private readonly Dictionary<string,IBotCommand> commandsDict;

    public BotHandler(
        ITelegramBotClient botClient,
        ReceiverOptions receiverOptions,
        ServiceManager serviceManager,
        IBotCommand[] commands,
        UserRegistrator userRegistrator)
    {
        this.botClient = botClient;
        this.receiverOptions = receiverOptions;
        this.serviceManager = serviceManager;
        this.commandsDict = commands.ToDictionary(command=>command.Name());
        this.userRegistrator = userRegistrator;
    }

    //================================CoolMethods===================================
    public async Task Start()
    {
        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

        await UserAttributesInfoManager.Initialize(serviceManager.SessionService);

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
        if (message.From.Username is not { } username) return;//хз
        var userId = message.From.Id;

        var role = await HandleMessageIfRegestration(message);
        
        if (message.Text is not { } messageText) return;
        var splittedMessage = messageText.Split();
        var chat = message.Chat;

        Console.WriteLine($"id: {userId}\nusername {username}\nроль: {role}"+(stateDict.Keys.Contains(userId)?("\nсостояние: "+ stateDict[userId]):""));

        //==================================================================================================

        await SetCommandsForUser(role,userId);

        if(role == Role.Unknown)
            return;

        if (stateDict.TryGetValue(userId,out var state))
        {
            TryExecuteCommand(commandsDict[state],update,chat, userId, role);
            return;
        }

        if (messageText[0] == '/' && commandsDict.TryGetValue(splittedMessage[0],out var command))
        {
            TryExecuteCommand(command,update,chat, userId, role);
            return;
        }
        await HandleSearching(chat, role, messageText, userId);
    }

    private async Task<Role> HandleMessageIfRegestration(Message message)
    {
        var role = serviceManager.RoleService.GetUserRoleByTgAsync(
            new TelegramInfo { TgUsername = message.From!.Username!, TgId = message.From.Id }).Result;
        if (role != Role.Unknown) return role;
        
        if (!stateDict.Keys.Contains(message.From.Id))
        {
            stateDict[message.From.Id] = "registration";
            await SendRegistrationMessage(message);
        }
        else if (stateDict[message.From.Id] == "registration")
        {
            role = userRegistrator.LogInAndRegisterUserAsync(message).Result;
            var answer = "для того чтобы пользоваться этим ботом вы должны быть участником лагеря ОЗШ";
            if (role != Role.Unknown)
            {
                answer = "регистрация завершена успешно";
                await SetCommandsForUser(role,message.From.Id);
            }
                
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

    private async Task HandleSearching(Chat chat, Role role, string messageText,long userId)
    {
        var users = await serviceManager.FindService.FindUserAsync(messageText);
        await SendResultMessage(users,chat,userId,role,messageText);
    }

    public async Task SendResultMessage(UserDomain[] users,Chat chat,long userId,Role role, string messageText)
    {
        if (users.Length==0)
        {
            await serviceManager.Logger.Log(userId,DateOnly.FromDateTime(DateTime.Now),false);
            await botClient.SendMessage(
                    chat.Id,
                    $"никто не найден",
                    replyMarkup: new ReplyKeyboardRemove(),
                    parseMode: ParseMode.MarkdownV2
                    );
        }
        else if (users.Length == 1)
        {
            await serviceManager.Logger.Log(userId,DateOnly.FromDateTime(DateTime.Now),true);
            if(role == Role.Counsellor)
                await botClient.SendMessage(
                    chat.Id,
                    users[0].FormateAnswer(role),
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData("Посещённые смены", "userSessions "+users[0].PhoneNumber),
                        InlineKeyboardButton.WithCallbackData("Редактировать", "edit "+users[0].PhoneNumber)
                    ),
                    parseMode: ParseMode.MarkdownV2
                    );
            else
                await botClient.SendMessage(
                    chat.Id,
                    users[0].FormateAnswer(role),
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData("Посещённые смены", "userSessions "+users[0].PhoneNumber)
                        ),
                    parseMode: ParseMode.MarkdownV2
                    );
        }
        else
        {
            await serviceManager.Logger.Log(userId,DateOnly.FromDateTime(DateTime.Now),false);
            await botClient.SendMessage(
                chat.Id,
                users.FormateAnswer(messageText),
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
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

        var role = serviceManager.RoleService.GetUserRoleByTgAsync(
            new TelegramInfo { TgUsername = callback.From.Username, TgId = callback.From.Id }).Result;

        Console.WriteLine($"id: {userId}\nusername {callback.From.Username }\nроль: {role}"+(stateDict.Keys.Contains(userId)?("\nсостояние: "+ stateDict[userId]):""));

        await SetCommandsForUser(role,userId);

        if(role == Role.Unknown)
            await botClient.SendMessage(
                chat.Id,
                "для того чтобы пользоваться этим ботом вы должны быть учавстником лагеря ОЗШ",
                replyMarkup: new ReplyKeyboardRemove());

        if (stateDict.TryGetValue(userId,out var state))
        {
            TryExecuteCommand(commandsDict[state],update,chat, userId, role);
        }
        else if (commandsDict.TryGetValue(splittedCommand[0],out var command))
        {
            TryExecuteCommand(command,update,chat, userId, role);
        }
        await botClient.AnswerCallbackQuery(callback.Id);
    }

    private async void TryExecuteCommand(IBotCommand command,Update update,Chat chat, long userId, Role role)
    {
        if (command.IsAvailible(role))
        {
            if (await command.ExecuteAsync(this,update))
                stateDict[userId] = command.Name();
            else
                stateDict.Remove(userId);
        }
        else if (command is IBotCommandWithState commandWithState)
        {
            await commandWithState.TryCancelState(botClient,chat,userId);
            stateDict.Remove(userId);
        }
    }



    //================================AdditionalMethods===============================

    public async Task SetCommandsForUser(Role role, long userId)
    {
        var commands = new List<BotCommand>();

        commands.AddRange(
            commandsDict.Keys
                .Where(command => command[0] == '/')
                .Where(command => commandsDict[command].IsAvailible(role))
                .Select(command => new BotCommand { Command = command.Substring(1), Description = commandsDict[command].GetDescription() }).ToArray()
        );

        await botClient.SetMyCommands(
            commands,
            scope: new BotCommandScopeChat { ChatId = userId });
    }
}