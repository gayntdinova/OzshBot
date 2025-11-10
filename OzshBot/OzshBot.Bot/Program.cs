using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System;
namespace OzshBot.Bot;


public enum UserRole
{
    User,
    Counselor,
    Admin
}

public class UserService
{
    private readonly Dictionary<long, UserRole> userRoles = new() { { 1307360984, UserRole.Admin } };

    public UserRole GetUserRole(long userId)
        => userRoles.TryGetValue(userId, out var role) ? role : UserRole.User;

    public void SetUserRole(long userId, UserRole role)
    {
        userRoles[userId] = role;
    }

    public bool IsCounselor(long userId)
    {
        return GetUserRole(userId) >= UserRole.Counselor;
    }
}

static class Program
{
    //клиент для работы с Bot API, позволяет отправлять сообщения, управлять ботом, подписываться на обновления и тд
    private static ITelegramBotClient botClient;

    //объект с настройками бота, здесь указываем какие типы Update будем получать, Timeout бота и тд
    private static ReceiverOptions receiverOptions;

    //сервис для работы с правами
    private static UserService userService;

    public static async Task SetCommandsForUser(long userId)
    {
        var commands = new List<BotCommand>();

        var role = userService.GetUserRole(userId);

        if (role == UserRole.Counselor || role == UserRole.Admin)
        {
            // Команды для вожатых и админов
            commands.AddRange(new[]
            {
                new BotCommand { Command = "promote", Description = "Выдать права вожатого" },
                new BotCommand { Command = "demote", Description = "Забрать права вожатого" },
                new BotCommand { Command = "list", Description = "Список вожатых" }
            });
        }

        // Общие команды для всех
        commands.AddRange(new[]
        {
            new BotCommand { Command = "help", Description = "Помощь" },
            new BotCommand { Command = "profile", Description = "Мой профиль" }
        });

        try
        {
            await botClient.SetMyCommands(
                commands,
                scope: new BotCommandScopeChat { ChatId = userId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка установки команд: {ex.Message}");
        }
    }

    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message is not { } message) return;
                    if (message.Text is not { } messageText) return;
                    var role = userService.GetUserRole(message.From.Id);

                    var chat = message.Chat;

                    Console.WriteLine($"id: {message.From.Id}\nusername {message.From.Username}\nроль: {userService.GetUserRole(message.From.Id)}");

                    if (messageText.StartsWith("/") == false)
                    {
                        await botClient.SendMessage(
                            chat.Id,
                            $"бабибо"
                            );
                    }
                    else if (messageText == "/start")
                    {
                        await SetCommandsForUser(message.From.Id);
                    }
                    else if (messageText == "/profile")
                    {
                        await botClient.SendMessage(
                            chat.Id,
                            $"ваш id: {message.From.Id}\nваш username {message.From.Username}\nваша роль: {userService.GetUserRole(message.From.Id)}"
                            );
                    }
                    else if(messageText.StartsWith("/promote") == true && (role == UserRole.Counselor || role == UserRole.Admin) ) 
                    {

                    }
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

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        Console.WriteLine(error switch
        {
            _ => error.ToString()
        });

        return Task.CompletedTask;
    }

    static async Task Main()
    {

        botClient = new TelegramBotClient("8445241215:AAE-fg7HdNllMonKukdR5T9e_8I4e4FwpXg");
        receiverOptions = new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message } };
        userService = new();

        using var cts = new CancellationTokenSource();

        botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

        Console.WriteLine($"{(await botClient.GetMe()).FirstName} запущен!");

        await Task.Delay(-1);
    }
}
