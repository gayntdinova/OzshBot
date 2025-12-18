using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Ninject;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services.Interfaces;
using OzshBot.Application.Services;
using OzshBot.Application.ToolsInterfaces;
using OzshBot.Bot.Commands;
using OzshBot.Bot.Extra;
using IBLogger = OzshBot.Application.ToolsInterfaces.ILogger;
using Microsoft.Extensions.Configuration;
using Ninject.Extensions.Conventions;
using OzshBot.Infrastructure.Parser;
using OzshBot.Infrastructure.Services;

namespace OzshBot.Bot;

static class Program
{
    static async Task Main()
    {
        var container = ConfigureContainer();
        var botHandler = container.Get<BotHandler>();
        await botHandler.Start();
    }

    private static StandardKernel ConfigureContainer()
    {
        var container = new StandardKernel();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var botToken = configuration["TelegramBot:Token"];

        //container.Bind<ITelegramBotClient>().ToConstant(new TelegramBotClient(botToken));
        container.Bind<ITelegramBotClient>().ToConstant(new TelegramBotClient("8445241215:AAE-fg7HdNllMonKukdR5T9e_8I4e4FwpXg"));
        
        container.Bind<ReceiverOptions>().ToConstant(new ReceiverOptions {
            AllowedUpdates = [UpdateType.Message,UpdateType.CallbackQuery]
        });
        container.Bind<ServiceManager>().ToSelf().InSingletonScope();
        
        container.Bind<IUserManagementService>().To<UserManagementService>().InSingletonScope();
        container.Bind<IUserRoleService>().To<UserRoleService>().InSingletonScope();
        container.Bind<IUserFindService>().To<UserFindService>().InSingletonScope();
        container.Bind<ISessionService>().To<SessionService>().InSingletonScope();

        container.Bind<ITableParser>().To<GoogleDocParser>().InSingletonScope();
        container.Bind<IUserRepository>().To<DbRepository>().InScope(ctx => ctx.Request);
        container.Bind<ISessionRepository>().To<SessionsRepository>().InScope(ctx => ctx.Request);
        container.Bind<IBLogger>().To<LogsRepository>().InScope(ctx => ctx.Request);

        container.Bind<MadeUpData>().ToConstant(new MadeUpData());
        
        container.Bind(x =>
            x.FromThisAssembly()
                .SelectAllClasses()
                .InheritedFrom<IBotCommand>()
                .BindAllInterfaces()
                .Configure(y => y.InTransientScope()));


        return container;
    }
}