using System.Text.RegularExpressions;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Bot.Extra;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace OzshBot.Bot.Commands;

public class SessionsCommand : IBotCommandWithState
{
    private readonly Role[] roles = [Role.Counsellor];
    private readonly Dictionary<long, SessionsState> stateDict = new();

    public string Name
        => "/sessions";

    public bool IsAvailable(Role role)
    {
        return roles.Contains(role);
    }

    public string Description
        => "Добавить / Изменить смену";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
        Update update)
    {
        var bot = botHandler.BotClient;
        var serviceManager = botHandler.ServiceManager;
        var formatter = botHandler.Formatter;

        switch (update.Type)
        {
            case UpdateType.Message:
                return await HandleMessage(update.Message!, bot, formatter, serviceManager);

            case UpdateType.CallbackQuery:
                return await HandleCallback(update.CallbackQuery!, bot, formatter, serviceManager);

            default:
                return false;
        }
    }

    private async Task<bool> HandleMessage(
        Message message,
        ITelegramBotClient bot,
        IFormatter formatter,
        ServiceManager serviceManager)
    {
        var chat = message.Chat;
        var userId = message.From!.Id;
        var text = message.Text!;

        var role = await serviceManager.RoleService.GetUserRoleByTgAsync(
            new TelegramInfo { TgId = userId, TgUsername = message.From.Username! });

        var sessions = await serviceManager.SessionService.GetAllSessionsAsync();

        if (stateDict.TryGetValue(userId, out var state))
        {
            state.messagesIds.Push(message.Id);
            state.MessagesToDelete += 1;

            if (state.Mode == SessionsMode.Edit)
            {
                //если мы нажали редактировать, ждём какое именно редактировать
                if (state.sessionToEdit == null)
                {
                    if (Regex.IsMatch(text,
                            @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}) (0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                    {
                        var splitted = text.Split(' ');
                        var start = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                        var end = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");

                        if (sessions.Any(session => session.SessionDates == new SessionDates(start, end)))
                        {
                            state.sessionToEdit = sessions.First(session =>
                                session.SessionDates == new SessionDates(start, end));

                            state.messagesIds.Push((await bot.SendMessage(
                                chat.Id,
                                formatter.FormatString("Введите новые даты смены в формате dd.MM.yyyy dd.MM.yyyy"),
                                ParseMode.MarkdownV2,
                                replyMarkup: new ReplyKeyboardRemove())).Id);
                            state.MessagesToDelete += 1;
                        }

                        return true;
                    }

                    state.messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        "Выберите из предложенных",
                        ParseMode.MarkdownV2,
                        replyMarkup: new ReplyKeyboardRemove())).Id);
                    state.MessagesToDelete += 1;
                    return true;
                }
                //если мы выбрали какую редактировать

                if (Regex.IsMatch(text,
                        @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}) (0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                {
                    var splitted = text.Split(' ');
                    var start = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                    var end = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");

                    state.sessionToEdit.SessionDates = new SessionDates(start, end);
                    var result = await serviceManager.SessionService.EditSessionAsync(state.sessionToEdit);

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
                            "Смена успешно изменена",
                            ParseMode.MarkdownV2,
                            replyMarkup: new ReplyKeyboardRemove());
                    await TryCancelState(bot, chat, userId);
                    return false;
                }

                state.messagesIds.Push((await bot.SendMessage(
                    chat.Id,
                    "Неверный формат даты",
                    ParseMode.MarkdownV2,
                    replyMarkup: new ReplyKeyboardRemove())).Id);
                state.MessagesToDelete += 1;
                return true;
            }

            if (state.Mode == SessionsMode.Add)
            {
                if (Regex.IsMatch(text,
                        @"^(0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2}) (0[1-9]|[12]\d|3[01])\.(0[1-9]|1[0-2])\.(19\d{2}|20\d{2})$"))
                {
                    var splitted = text.Split(' ');
                    var start = DateOnly.ParseExact(splitted[0], "dd.MM.yyyy");
                    var end = DateOnly.ParseExact(splitted[1], "dd.MM.yyyy");
                    var result = await serviceManager.SessionService.AddSessionAsync(new SessionDates(start, end));

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
                            "Смена успешно добавлена",
                            ParseMode.MarkdownV2,
                            replyMarkup: new ReplyKeyboardRemove());

                    await TryCancelState(bot, chat, userId);
                    return false;
                }
                else
                {
                    state.messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        "Неверный формат даты",
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2)).Id);
                    state.MessagesToDelete += 1;
                    return true;
                }
            }

            await TryCancelState(bot, chat, userId);
            return false;
        }
        //если написали /sessions

        await TryCancelState(bot, chat, userId);

        stateDict[userId] = new SessionsState();
        stateDict[userId].messagesIds.Push((await bot.SendMessage(
            chat.Id,
            formatter.FormatSessions(sessions),
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Добавить", "sessionsAdd"),
                    InlineKeyboardButton.WithCallbackData("Редактировать", "sessionsEdit")
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Отмена", "sessionsCancel")
                }
            }),
            parseMode: ParseMode.MarkdownV2
        )).Id);

        return true;
    }


    private async Task<bool> HandleCallback(
        CallbackQuery callback,
        ITelegramBotClient bot,
        IFormatter formatter,
        ServiceManager serviceManager)
    {
        var chat = callback.Message!.Chat;
        var userId = callback.From.Id;
        var data = callback.Data!;

        if (stateDict.TryGetValue(userId, out var state))
            switch (data)
            {
                case "sessionsAdd":
                    while (state.MessagesToDelete > 0)
                    {
                        await bot.DeleteMessage(chat, state.messagesIds.Pop());
                        state.MessagesToDelete -= 1;
                    }

                    state.sessionToEdit = null;
                    state.Mode = SessionsMode.Add;
                    state.messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        formatter.FormatString("Введите даты новой смены в формате dd.MM.yyyy dd.MM.yyyy"),
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                    )).Id);
                    state.MessagesToDelete += 1;
                    return true;

                case "sessionsEdit":
                    while (state.MessagesToDelete > 0)
                    {
                        await bot.DeleteMessage(chat, state.messagesIds.Pop());
                        state.MessagesToDelete -= 1;
                    }

                    state.Mode = SessionsMode.Edit;

                    state.messagesIds.Push((await bot.SendMessage(
                        chat.Id,
                        formatter.FormatString("Выберите смену для редактирования:"),
                        replyMarkup: await GetSessionsKeyboard(serviceManager.SessionService),
                        parseMode: ParseMode.MarkdownV2
                    )).Id);
                    state.MessagesToDelete += 1;
                    return true;

                default:
                    await TryCancelState(bot, chat, userId);
                    return false;
            }

        return false;
    }

    private async Task<ReplyKeyboardMarkup> GetSessionsKeyboard(ISessionService sessionService)
    {
        return new ReplyKeyboardMarkup(
                (await sessionService
                    .GetAllSessionsAsync())
                .Select(session => new KeyboardButton[]
                {
                    new(
                        $"{session.SessionDates.StartDate.ToString("dd.MM.yyyy")} {session.SessionDates.EndDate.ToString("dd.MM.yyyy")}")
                }))
            { ResizeKeyboard = true };
    }

    public async Task TryCancelState(ITelegramBotClient bot, Chat chat, long userId)
    {
        if (stateDict.ContainsKey(userId))
        {
            while (stateDict[userId].messagesIds.Count != 0)
                await bot.DeleteMessage(chat, stateDict[userId].messagesIds.Pop());
            stateDict.Remove(userId);
        }
    }

    private enum SessionsMode
    {
        None,
        Edit,
        Add
    }

    private class SessionsState
    {
        public Session? sessionToEdit = null;
        public SessionsMode Mode = SessionsMode.None;
        public readonly Stack<MessageId> messagesIds = new();
        public int MessagesToDelete = 0;
    }
}