using System.Text.RegularExpressions;
using OzshBot.Bot.Extra;
using OzshBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OzshBot.Bot.Commands;

public class DeleteCommand : IBotCommand
{
    private readonly Role[] roles = [Role.Counsellor];

    public string Name
        => "/delete";

    public bool IsAvailable(Role role)
    {
        return roles.Contains(role);
    }

    public string Description
        => "Удалить информацию о пользователе";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
        Update update)
    {
        var bot = botHandler.BotClient;
        var serviceManager = botHandler.ServiceManager;

        var message = update.Message!;
        var messageText = message.Text!;
        var chat = message.Chat;

        var splitted = messageText.Split(" ");

        if (splitted.Length != 2 ||
            !Regex.IsMatch(splitted[1], @"^(\+7|8)\s?\(?\d{3}\)?[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}$"))
        {
            await bot.SendMessage(
                chat.Id,
                "Использование: /delete номер телефона",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
            );
            return false;
        }

        var result = await serviceManager.ManagementService.DeleteUserAsync(splitted[1]);
        if (result.IsFailed)
            await bot.SendMessage(
                chat.Id,
                result.Errors.First().GetExplanation(),
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
            );
        else
            await bot.SendMessage(
                chat.Id,
                $"Пользователь успешно удалён",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
            );
        return false;
    }
}