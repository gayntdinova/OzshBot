using System.Threading.Tasks;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;
using OzshBot.Infrastructure.Data;
using OzshBot.Infrastructure.Services;

namespace OzshBot.Infrastructure.DbTest;

class Program
{
    static async Task Main(string[] args)
    {
        var dbContext = AppDbContextFactory.CreateContext();
        var dbRepository = new DbRepository(dbContext);
        await ModifyUserFunc(dbRepository);
    }

    private async static Task AddUserFunc(DbRepository dbRepository)
    {
        var counsellor = new CounsellorInfo
        {
            FullName = new FullName { Name = "Иван", Surname = "Иванов", Patronymic = "Иванович" },
            Email = "ivan.ivanov@ivan.iv",
            PhoneNumber = "1234567890",
            City = "Иваново",
            Birthday = new DateOnly(2000, 1, 1),
            Group = 0,
            Sessions = []
        };
        var user = new User
        {
            TelegramInfo = new TelegramInfo { TgUsername = "ivan_ivanov" },
            CounsellorInfo = counsellor,
            ChildInfo = null,
            Role = Domain.Enums.Role.Counsellor
        };
        await dbRepository.AddUserAsync(user);
    }

    private async static Task ModifyUserFunc(DbRepository dbRepository)
    {
        var user = await dbRepository.GetUserByTgAsync(new TelegramInfo { TgUsername = "ivan_ivanov" });
        user.TelegramInfo.TgId = 1234567890;
        await dbRepository.UpdateUserAsync(user);
    }
}

