using OzshBot.Bot.Extra;
using OzshBot.Domain.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UserDomain = OzshBot.Domain.Entities.User;

namespace OzshBot.Bot.Commands;

public class EditCommand : IBotCommandWithState
{
    private readonly Role[] roles = [Role.Counsellor];
    private readonly Dictionary<long, EditState> stateDict = new();

    public string Name
        => "edit";

    public bool IsAvailable(Role role)
    {
        return roles.Contains(role);
    }

    public string Description
        => "";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
        Update update)
    {
        var bot = botHandler.BotClient;
        var serviceManager = botHandler.ServiceManager;
        var formatter = botHandler.Formatter;

        switch (update.Type)
        {
            case UpdateType.Message:
                var message = update.Message!;
                var messageText = message.Text!;
                var userId = message.From!.Id;
                var chat = message.Chat;

                //если уже находится в ожидании какого то ответа
                if (stateDict.TryGetValue(update.Message!.From!.Id, out var state))
                {
                    //если во время редактирования, то записываем сообщения в удаляемые
                    state.messagesIds.Push(message.Id);
                    state.MessagesToDelete += 1;

                    //если мы ждём нажатия на кнопку, то отменяем всё 
                    if (state.WaitingSelectField)
                    {
                        await TryCancelState(bot, chat, userId);
                        return false;
                    }


                    var attributeInfo = state.UserAttribute.GetInfo();

                    //если подходит под регулярку этого аттрибута то перекидываем на следующий или заканчиваем
                    if (await attributeInfo.CorrectFormateFunction(messageText))
                    {
                        state.MessagesToDelete = 0;
                        attributeInfo.FillingAction(state.EditUser, messageText);
                        state.WaitingSelectField = true;

                        state.messagesIds.Push((await bot.SendMessage(
                            chat.Id,
                            "Изменено",
                            replyMarkup: new ReplyKeyboardRemove())).Id);
                        return true;
                    }

                    //если не подходит под регулярку то переспрашиваем
                    await SendStateInfoMessage(bot,formatter, chat, stateDict[userId], state.UserAttribute, true);
                    return true;
                }

                return false;

            case UpdateType.CallbackQuery:
                var callback = update.CallbackQuery!;
                var splitted = callback.Data!.Split();
                var chat1 = callback.Message!.Chat;
                var userId1 = callback.From.Id;
                switch (splitted[0])
                {
                    case "edit":
                        return await HandleEditMenu(bot, serviceManager, chat1, userId1, splitted[1]);

                    case "editTheme":
                        return await HandleEditThemes(bot, formatter, chat1, callback.From.Id,
                            (UserAttribute)int.Parse(splitted[1]));

                    case "editApply":
                        if (!stateDict.TryGetValue(userId1, out var state1))
                            return false; //невозможный в теории случай но я на всякий оставлю

                        var result = await serviceManager.ManagementService.EditUserAsync(state1.EditUser);

                        if (result.IsFailed)
                        {
                            await bot.SendMessage(
                                chat1.Id,
                                result.Errors.First().GetExplanation(),
                                replyMarkup: new ReplyKeyboardRemove(),
                                parseMode: ParseMode.MarkdownV2
                            );
                        }
                        else
                        {
                            await bot.SendMessage(
                                chat1.Id,
                                $"Пользователь успешно отредактирован",
                                replyMarkup: new ReplyKeyboardRemove(),
                                parseMode: ParseMode.MarkdownV2
                            );
                            await botHandler.SendResultMessage(new UserDomain[] { state1.EditUser }, chat1, userId1,
                                Role.Counsellor, "");
                        }

                        await TryCancelState(bot, chat1, userId1);
                        return false;

                    default:
                        await TryCancelState(bot, chat1, userId1);
                        return false;
                }
            default:
                return false;
        }
    }

    private async Task<bool> HandleEditMenu(ITelegramBotClient bot, ServiceManager serviceManager, Chat chat,
        long userId, string phoneNumber)
    {
        await TryCancelState(bot, chat, userId);

        var editedUser = await serviceManager.FindService.FindUserByPhoneNumberAsync(phoneNumber);

        if (editedUser == null)
        {
            await bot.SendMessage(
                chat.Id,
                "Телефон этого человека сменился или его уже не существует",
                replyMarkup: new ReplyKeyboardRemove(),
                parseMode: ParseMode.MarkdownV2
            );
            return false;
        }
        else
        {
            var state = new EditState(editedUser);
            state.messagesIds.Push((await bot.SendMessage(
                chat.Id,
                "Выбирите что редактировать, если вы напишете сообщение не по теме, редактирование отменится",
                replyMarkup: CreateKeyboard(editedUser.Role),
                parseMode: ParseMode.MarkdownV2
            )).Id);

            stateDict[userId] = state;
            return true;
        }
    }

    private async Task<bool> HandleEditThemes(ITelegramBotClient bot,IFormatter formatter, Chat chat, long userId, UserAttribute attribute)
    {
        if (!stateDict.Keys.Contains(userId)) return false; //невозможный в теории случай но я на всякий оставлю

        var state = stateDict[userId];
        if (!state.WaitingSelectField)
            while (state.MessagesToDelete > 0)
            {
                await bot.DeleteMessage(chat, state.messagesIds.Pop());
                state.MessagesToDelete -= 1;
            }

        await SendStateInfoMessage(bot,formatter, chat, state, attribute, false);
        state.WaitingSelectField = false;
        return true;
    }

    private InlineKeyboardMarkup CreateKeyboard(Role role)
    {
        var result = new List<InlineKeyboardButton[]>();

        var editableWithRole = UserAttributesInfoManager.EditableAttributes
            .Where(attr => role.ImplementsAttribute(attr)).ToArray();

        for (var i = 0; i < editableWithRole.Length; i += 2)
        {
            var attribute = UserAttributesInfoManager.EditableAttributes[i];

            var first = UserAttributesInfoManager.EditableAttributes[i];
            var second = UserAttributesInfoManager.EditableAttributes[i + 1];

            result.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(first.GetInfo().Name, $"editTheme {(int)first}"),
                InlineKeyboardButton.WithCallbackData(second.GetInfo().Name, $"editTheme {(int)second}")
            });
        }

        if (editableWithRole.Length % 2 == 1)
        {
            var last = editableWithRole[editableWithRole.Length - 1];
            result.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData(last.GetInfo().Name, $"editTheme {(int)last}")
            });
        }

        result.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("Отмена", "editCancel")
        });
        result.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("Применить", "editApply")
        });
        return result.ToArray();
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

    private async Task SendStateInfoMessage(ITelegramBotClient bot,IFormatter formatter, Chat chat, EditState state, UserAttribute attribute,
        bool wasIncorrect)
    {
        var attributeInfo = attribute.GetInfo();
        ReplyMarkup markup = attributeInfo.KeyboardMarkup != null
            ? await attributeInfo.KeyboardMarkup(state.EditUser)
            : new ReplyKeyboardRemove();

        state.messagesIds.Push((await bot.SendMessage(
            chat.Id,
            formatter.FormatString((wasIncorrect ? "Некорректный формат\n" : "") + attributeInfo.WritingInfo),
            ParseMode.MarkdownV2,
            replyMarkup: markup
        )).Id);
        state.UserAttribute = attribute;
        state.MessagesToDelete += 1;
    }

    private class EditState
    {
        public bool WaitingSelectField = true;
        public UserAttribute UserAttribute;
        public readonly UserDomain EditUser;
        public readonly Stack<MessageId> messagesIds = new();
        public int MessagesToDelete = 0;

        public EditState(UserDomain editUser)
        {
            EditUser = editUser;
        }
    }
}