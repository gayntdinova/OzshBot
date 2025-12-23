using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using UserDomain = OzshBot.Domain.Entities.User;
using Telegram.Bot.Types.ReplyMarkups;
using System.Data;
using OzshBot.Bot.Commands;
using OzshBot.Bot.Extra;

namespace OzshBot.Bot;


public class BotHandler
{
    private readonly ReceiverOptions receiverOptions;

    private readonly UserRegistrator userRegistrator;

    private Dictionary<long,string> stateDict = new();

    private readonly Dictionary<string,IBotCommand> commandsDict;

    public readonly ITelegramBotClient BotClient;

    public readonly ServiceManager ServiceManager;

    public BotHandler(
        ITelegramBotClient botClient,
        ReceiverOptions receiverOptions,
        ServiceManager serviceManager,
        IBotCommand[] commands,
        UserRegistrator userRegistrator)
    {
        BotClient = botClient;
        this.receiverOptions = receiverOptions;
        ServiceManager = serviceManager;
        commandsDict = commands.ToDictionary(command=>command.Name);
        this.userRegistrator = userRegistrator;
    }

    public async Task Start()
    {
        using var cts = new CancellationTokenSource();

        BotClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

        await UserAttributesInfoManager.Initialize(ServiceManager.SessionService);

        Console.WriteLine($"{(await BotClient.GetMe()).FirstName} запущен!");

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

    private async Task HandleMessage(Update update)
    {
        if (update.Message is not { } message) return;
        if (message.From == null) return;//хз
        if (message.From.Username is not { } username) return;//хз
        var userId = message.From.Id;

        var role = await HandleMessageIfRegistration(message);
        
        if (message.Text is not { } messageText) return;
        var splittedMessage = messageText.Split();
        var chat = message.Chat;

        Console.WriteLine($"id: {userId}\nusername {username}\nроль: {role}"+(stateDict.Keys.Contains(userId)?("\nсостояние: "+ stateDict[userId]):""));

        await SetCommandsForUser(role,userId);

        if(role == Role.Unknown)
            return;

        if (stateDict.TryGetValue(userId,out var state))
        {
            await TryExecuteCommand(commandsDict[state],update,chat, userId, role);
            return;
        }

        if (messageText[0] == '/' && commandsDict.TryGetValue(splittedMessage[0],out var command))
        {
            await TryExecuteCommand(command,update,chat, userId, role);
            return;
        }
        await HandleSearching(chat, role, messageText, userId);
    }

    private async Task<Role> HandleMessageIfRegistration(Message message)
    {
        var role = ServiceManager.RoleService.GetUserRoleByTgAsync(
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
                
            await BotClient.SendMessage(message.Chat.Id, answer, parseMode: ParseMode.MarkdownV2);
            stateDict.Remove(message.From.Id);
        }
        return role;
    }
    
    
    private async Task SendRegistrationMessage(Message message)
    {
        if (message.Text != null)
        {
            var button = new KeyboardButton("Отправить контакт") { RequestContact = true };

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { button }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };
        
            await BotClient.SendMessage(
                message.Chat.Id,
                "Пожалуйста, отправьте ваш контакт:",
                replyMarkup: keyboard
            );
        }
    }

    private async Task HandleSearching(Chat chat, Role role, string messageText,long userId)
    {
        var users = await ServiceManager.FindService.FindUserAsync(messageText);
        await SendResultMessage(users,chat,userId,role,messageText);
    }

    public async Task SendResultMessage(UserDomain[] users,Chat chat,long userId,Role role, string messageText)
    {
        if (users.Length==0)
        {
            await ServiceManager.Logger.Log(userId,false);
            await BotClient.SendMessage(
                chat.Id,
                $"Никто не найден",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
        }
        else if (users.Length == 1)
        {
            await ServiceManager.Logger.Log(userId,true);
            if(role == Role.Counsellor)
                await BotClient.SendMessage(
                    chat.Id,
                    users[0].FormateAnswer(role),
                    replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithCallbackData("Посещённые смены", "userSessions "+users[0].PhoneNumber),
                        InlineKeyboardButton.WithCallbackData("Редактировать", "edit "+users[0].PhoneNumber)
                    ),
                    parseMode: ParseMode.MarkdownV2
                    );
            else
                await BotClient.SendMessage(
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
            await ServiceManager.Logger.Log(userId,true);
            await BotClient.SendMessage(
                chat.Id,
                users.FormateAnswer(messageText),
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
        }
    }

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

        var role = ServiceManager.RoleService.GetUserRoleByTgAsync(
            new TelegramInfo { TgUsername = callback.From.Username, TgId = callback.From.Id }).Result;

        Console.WriteLine($"id: {userId}\nusername {callback.From.Username }\nроль: {role}"+(stateDict.Keys.Contains(userId)?("\nсостояние: "+ stateDict[userId]):""));

        await SetCommandsForUser(role,userId);

        if(role == Role.Unknown)
            await BotClient.SendMessage(
                chat.Id,
                "для того чтобы пользоваться этим ботом вы должны быть учавстником лагеря ОЗШ",
                replyMarkup: new ReplyKeyboardRemove());

        if (stateDict.TryGetValue(userId,out var state))
        {
            await TryExecuteCommand(commandsDict[state],update,chat, userId, role);
        }
        else if (commandsDict.TryGetValue(splittedCommand[0],out var command))
        {
            await TryExecuteCommand(command,update,chat, userId, role);
        }
        await BotClient.AnswerCallbackQuery(callback.Id);
    }

    private async Task TryExecuteCommand(IBotCommand command,Update update,Chat chat, long userId, Role role)
    {
        if (command.IsAvailable(role))
        {
            if (await command.ExecuteAsync(this,update))
                stateDict[userId] = command.Name;
            else
                stateDict.Remove(userId);
        }
        else if (command is IBotCommandWithState commandWithState)
        {
            await BotClient.SendMessage(
                chat.Id,
                $"У вас нет прав пользоваться этой командой",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
                );
            await commandWithState.TryCancelState(BotClient,chat,userId);
            stateDict.Remove(userId);
        }
    }



    private async Task SetCommandsForUser(Role role, long userId)
    {
        var commands = new List<BotCommand>();

        commands.AddRange(
            commandsDict.Keys
                .Where(command => command[0] == '/')
                .Where(command => commandsDict[command].IsAvailable(role))
                .Select(command => new BotCommand { Command = command.Substring(1), Description = commandsDict[command].Description }).ToArray()
        );

        await BotClient.SetMyCommands(
            commands,
            scope: new BotCommandScopeChat { ChatId = userId });
    }
}