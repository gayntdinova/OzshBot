using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using OzshBot.Domain.Enums;
using OzshBot.Domain.Entities;
using Telegram.Bot.Types.ReplyMarkups;
namespace OzshBot.Bot;


public class UserSessionsCommand : IBotCommand
{
    private readonly Role[] roles = new[]{Role.Child, Role.Counsellor};
    public string Name
    => "userSessions";

    public bool IsAvailable(Role role)
    => roles.Contains(role);

    public string Description
    => "";

    public async Task<bool> ExecuteAsync(BotHandler botHandler,
                                        Update update)
    {
        var bot = botHandler.BotClient;
        var serviceManager = botHandler.ServiceManager;
        
        switch (update.Type)
        {
            case UpdateType.CallbackQuery:
                var callback = update.CallbackQuery!;
                var splitted = callback.Data!.Split();
                var chat = callback.Message!.Chat;

                var user = await serviceManager.FindService.FindUserByPhoneNumberAsync(splitted[1]);

                if (user==null)
                {
                    await bot.SendMessage(
                        chat.Id,
                        "Телефон этого человека сменился или его уже не существует",
                        replyMarkup: new ReplyKeyboardRemove(),
                        parseMode: ParseMode.MarkdownV2
                        );
                    return false;
                }

                var sessions = new Session[0];
                if (user.ChildInfo != null)
                    sessions = sessions.Concat(user.ChildInfo.Sessions).ToArray();
                if (user.CounsellorInfo != null)
                    sessions = sessions.Concat(user.CounsellorInfo.Sessions).ToArray();

                await bot.SendMessage(
                    chat.Id,
                    sessions.FormateAnswer(user),
                    replyMarkup: new ReplyKeyboardRemove(),
                    parseMode: ParseMode.MarkdownV2
                );
                return false;
            
        }
        return false;
    }
}

