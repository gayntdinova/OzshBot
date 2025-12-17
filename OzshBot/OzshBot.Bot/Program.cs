using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Ninject;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Application.Services;
using OzshBot.Application.ToolsInterfaces;
namespace OzshBot.Bot;

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
        container.Bind<ReceiverOptions>().ToConstant(new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message,UpdateType.CallbackQuery } });
        container.Bind<UserService>().ToSelf();
        container.Bind<IUserManagementService>().To<UserManagementService>();
        container.Bind<ISessionService>().To<SessionService>();
        container.Bind<ISessionRepository>().To<MySessionRepository>();
        container.Bind<IUserRoleService>().To<MyUserRoleService>();
        container.Bind<ITableParser>().To<MyTableParser>();
        container.Bind<IUserFindService>().To<UserFindService>();
        container.Bind<IUserRepository>().To<MyUserRepository>();
        container.Bind<MadeUpData>().ToConstant(new MadeUpData());

        container.Bind<IBotCommand>().To<AddCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<DeleteCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<EditCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<HelpCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<ProfileCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<PromoteCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<LoadCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<GroupCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<ClassCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<UserSessionsCommand>().InSingletonScope();
        container.Bind<IBotCommand>().To<SessionsCommand>().InSingletonScope();

        return container;
    }
}