using System.Threading.Tasks;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
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
        var res = await dbRepository.GetUserByTgAsync(new TelegramInfo { TgUsername = "student_ozsh" });
    }

    static async Task AddStudent(DbRepository dbRepository)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = new FullName
            {
                Name = "Ученик",
                Surname = "ВТорой",
                Patronymic = null
            },
            TelegramInfo = new TelegramInfo
            {
                TgUsername = "student_2",
                TgId = 1200000000
            },
            Email = "stdent2@ozsh.ru",
            PhoneNumber = "+79992234567",
            City = "Екатеринбург",
            Birthday = new DateOnly(2010, 5, 15),
            Role = Role.Child,
            ChildInfo = new ChildInfo
            {
                Group = 1,
                EducationInfo = new EducationInfo
                {
                    School = "СУНЦ",
                    Class = 9
                }
            }
        };
        await dbRepository.AddUserAsync(user);
    }

    static async Task ModifyStudent(DbRepository dbRepository)
    {
        var user = await dbRepository.GetUserByTgAsync(new TelegramInfo { TgUsername = "student_ozsh", TgId = 1000000000 });
        user.ChildInfo.ContactPeople.Add(new ContactPerson
        {
            FullName = new FullName
            {
                Name = "Мама",
                Surname = "Озш"
            },
            PhoneNumber = "+79001000000"
        });
        await dbRepository.UpdateUserAsync(user);
    }
}

