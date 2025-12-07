using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Ninject;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Application.Services;
using Microsoft.Extensions.Configuration;

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

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var botToken = configuration["TelegramBot:Token"];

        container.Bind<ITelegramBotClient>().ToConstant(new TelegramBotClient(botToken));
        container.Bind<ReceiverOptions>().ToConstant(new ReceiverOptions { AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery } });
        container.Bind<UserService>().ToSelf();
        container.Bind<IUserManagementService>().To<UserManagementService>();
        container.Bind<ITableParser>().ToConstant(new MyTableParser());
        container.Bind<IUserRoleService>().To<MyUserRoleService>();
        container.Bind<IUserFindService>().To<UserFindService>();
        container.Bind<IUserRepository>().To<MyUserRepository>();
        container.Bind<MadeUpData>().ToConstant(new MadeUpData());

        return container;
    }
}