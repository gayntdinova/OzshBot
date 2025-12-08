using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Ninject;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Application.Services;
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
        container.Bind<ITableParser>().ToConstant(new MyTableParser());
        container.Bind<IUserRoleService>().To<UserRoleService>();
        container.Bind<IUserFindService>().To<UserFindService>();
        container.Bind<IUserRepository>().To<MyUserRepository>();
        container.Bind<MadeUpData>().ToConstant(new MadeUpData());

        return container;
    }
}