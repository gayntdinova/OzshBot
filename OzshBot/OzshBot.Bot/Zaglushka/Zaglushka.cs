using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Ninject;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Formats.Asn1;
using FluentResults;
using OzshBot.Application.Services.Interfaces;
using UserDomain = OzshBot.Domain.Entities.User;
using OzshBot.Application.RepositoriesInterfaces;
using Telegram.Bot.Types.Passport;
using OzshBot.Application.ToolsInterfaces;
namespace OzshBot.Bot;


public class MadeUpData
{
    private static readonly string[] Towns =
    {
        "Москва", "Санкт-Петербург", "Казань", "Новосибирск", "Екатеринбург",
        "Нижний Новгород", "Самара", "Ростов-на-Дону", "Уфа", "Красноярск",
        "Воронеж", "Пермь", "Волгоград", "Краснодар", "Саратов",
        "Тюмень", "Иркутск", "Барнаул", "Омск", "Тула"
    };
    private static readonly string[] Schools =
    {
        "Школа №1", "Школа №5", "Школа №7", "Школа №12", "Школа №15",
        "Гимназия №3", "Гимназия №6", "Лицей №10", "Лицей №2", "Лицей №8",
        "Школа №18", "Школа №20", "Школа №25", "Школа №33", "Школа №40",
        "Гимназия №17", "Лицей №23", "Школа №48", "Школа №52", "Сунц УрФУ"
    };
    private static readonly string[] Names =
    {
        "Алексей", "Михаил", "Дмитрий", "Иван", "Егор",
        "Владислав", "Кирилл", "Сергей", "Никита", "Артём",
        "София", "Анна", "Мария", "Екатерина", "Полина",
        "Валерия", "Алиса", "Дарья", "Ксения", "Вика"
    };
    private static readonly string[] Surnames =
    {
        "Иванов", "Петров", "Сидоров", "Кузнецов", "Смирнов",
        "Попов", "Васильев", "Новиков", "Фёдоров", "Морозов",
        "Волков", "Алексеев", "Лебедев", "Скоробохатько", "Егоров",
        "Кириллов", "Макаров", "Николаев", "Захаров", "Зайцев"
    };
    private static readonly string[] Patronymics =
    {
        "Алексеевич", "Михайлович", "Дмитриевич", "Иванович", "Егорович",
        "Владиславович", "Кириллович", "Матвеевич", "Никитович", "Артёмович",
        "Алексеевна", "Михайловна", "Дмитриевна", "Ивановна", "Егоровна",
        "Владиславовна", "Кирилловна", "Матвеевна", "Никитовна", "Артёмовна"
    };

    public List<UserDomain> Users;
    public List<Session> Sessions;

    public MadeUpData()
    {
        Sessions = new();
        var date = new DateOnly().AddYears(2002);
        for(var i = 0; i < 5; i++)
        {
            date = date.AddDays(200);
            Sessions.Add(new Session{SessionDates = new SessionDates(date,date.AddDays(100))});
        }

        Users = new()
        {
            new UserDomain
            {

                FullName = new
                (
                    "Абоба",
                    "Абобович",
                    "Абобин"
                ),
                Birthday = new DateOnly(),
                City = "Екатеринбург",
                PhoneNumber = $"+79326189209",
                Email = $"child@child.com",
                CounsellorInfo = null,
                ChildInfo = new()
                {
                    EducationInfo = new EducationInfo
                    {
                        Class = 11,
                        School = "Сунц УрФУ"
                    },
                    Group = 1000,
                    Sessions = new HashSet<Session> {Sessions[Sessions.Count()-1] },
                    ContactPeople = new HashSet<ContactPerson>{}
                },
                TelegramInfo = null,
                Role = Role.Counsellor
            },
            new UserDomain
            {

                FullName = new FullName
                (
                    "Сергей",
                    "Сергеевич",
                    "Сергеев"
                ),
                Birthday = new DateOnly(),
                City = "Екатеринбург",
                PhoneNumber = $"+79221517046",
                Email = $"child@child.com",
                CounsellorInfo = new()
                {
                    Group = 1000,
                    Sessions = new HashSet<Session> {Sessions[0],Sessions[1] },
                },
                ChildInfo = null,
                TelegramInfo = null,
                Role = Role.Counsellor
            }
        };
        var rand = new Random();
        for(var i = 0; i < 100; i++)
        {
            Users.Add(GenerateRandomChild(rand));
            Users.Add(GenerateRandomCounsellor(rand));
        }
    }

    private int id = 0;
    private UserDomain GenerateRandomChild(Random random)
    => new UserDomain
    {
        FullName = GenerateRandomFullName(random),
        Birthday = new DateOnly(),
        City = Towns[random.Next(Towns.Length)],
        PhoneNumber = $"+79{random.Next(100000000, 999999999)}",
        Email = $"child{id}@child{id}.com",
        TelegramInfo = new TelegramInfo
        {
            TgId = id++,
            TgUsername = "child" + id.ToString()
        },
        CounsellorInfo = null,
        ChildInfo = new ChildInfo
        {
            EducationInfo = new EducationInfo
            {
                Class = random.Next(1, 11),
                School = Schools[random.Next(Schools.Length)]
            },
            Group = 1000 + id,
            Sessions = GenerateRandomVisitedSessions(random),
            ContactPeople = GenerateContactPersonList(random)
        },
        Role = Role.Child
    };

    private UserDomain GenerateRandomCounsellor(Random random)
    => new UserDomain
    {
        FullName = GenerateRandomFullName(random),
        Birthday = new DateOnly(),
        City = Towns[random.Next(Towns.Length)],
            
        PhoneNumber = $"+79{random.Next(100000000, 999999999)}",
        Email = $"counsellor{id}@counsellor{id}.com",
        TelegramInfo = new TelegramInfo
        {
            TgId = id++,
            TgUsername = "counsellor" + id.ToString()
        },
        ChildInfo = null,
        CounsellorInfo = new CounsellorInfo
        {
            Group = 1000 + id,
            Sessions = GenerateRandomVisitedSessions(random)
        },
        Role = Role.Counsellor
    };

    private HashSet<ContactPerson> GenerateContactPersonList(Random random)
    {
        var length = random.Next(0, 2);
        var list = new List<ContactPerson>();
        for (int i = 0; i < length; i++)
        {
            list.Add(new ContactPerson
            {
                FullName = GenerateRandomFullName(random),
                Id = new Guid(),
                PhoneNumber = $"+79{random.Next(100000000, 999999999)}"
            });
        }
        return list.ToHashSet();
    }

    private FullName GenerateRandomFullName(Random random)
    => new FullName
    (
        Names[random.Next(Names.Length)],
        Surnames[random.Next(Names.Length)],
        Patronymics[random.Next(Names.Length)]
    );

    private HashSet<Session> GenerateRandomVisitedSessions(Random random)
    {
        var numberOfSessions = random.Next(1,Sessions.Count());
        return  Sessions
            .OrderBy(_ => random.Next())
            .Take(numberOfSessions)
            .ToHashSet();
    }
}

public class MyUserRepository : IUserRepository
{
    private readonly MadeUpData madeUpData;
    public MyUserRepository(MadeUpData madeUpData)
    {
        this.madeUpData = madeUpData;
    }

    public async Task AddUserAsync(UserDomain user)
    {
        madeUpData.Users.Add(user);
    }

    public async Task DeleteUserAsync(string phoneNumber)
    {
        madeUpData.Users.Remove(madeUpData.Users.First(user=>user.PhoneNumber == phoneNumber));
    }

    public async Task<UserDomain?> GetUserByTgAsync(TelegramInfo telegramInfo)
    {
        var users = madeUpData.Users.Where(user => user.TelegramInfo?.TgUsername.ToLower() == telegramInfo.TgUsername.ToLower());

        return users.FirstOrDefault();
    }

    public async Task<UserDomain?> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        var users = madeUpData.Users.Where(user => user.PhoneNumber?.ToLower() == phoneNumber.ToLower());

        return users.FirstOrDefault();
    }

    public async Task<UserDomain[]?> GetUsersByClassAsync(int classNumber)
    {
        Console.WriteLine(classNumber);
        var users = madeUpData.Users.Where(user =>user.ChildInfo?.EducationInfo?.Class == classNumber);
        
        return users.Any()?users.ToArray():null;
    }

    public async Task<UserDomain[]?> GetUsersByFullNameAsync(NameSearch fullName)
    {
        var users = madeUpData.Users;

        if (fullName.Name != null)
            users = users.Where(user 
            => user.FullName.Name?.ToLower() == fullName.Name.ToLower()).ToList();

        if (fullName.Surname != null)
            users = users.Where(user 
            => user.FullName.Surname?.ToLower() == fullName.Surname.ToLower()).ToList();

        if (fullName.Patronymic != null)
            users = users.Where(user 
            => user.FullName.Patronymic?.ToLower() == fullName.Patronymic.ToLower()).ToList();
        
        return users.Any()?users.ToArray():null;
    }

    public async Task<UserDomain[]?> GetUsersByGroupAsync(int group)
    {
        var users = madeUpData.Users.Where(user 
            => (user.CounsellorInfo?.Group == group)||(user.ChildInfo?.Group == group));

        return users.Any()?users.ToArray():null;
    }

    public async Task<UserDomain[]?> GetUsersByCityAsync(string city)
    {
        var users = madeUpData.Users.Where(user 
            => user.City!=null && user.City.ToLower() == city.ToLower());

        return users.Any()?users.ToArray():null;
    }

    public async Task<UserDomain[]?> GetUsersBySchoolAsync(string school)
    {
        var users = madeUpData.Users.Where(user 
            => user.ChildInfo?.EducationInfo?.School.ToLower() == school.ToLower());

        return users.Any()?users.ToArray():null;
    }

    public async Task UpdateUserAsync(UserDomain user)
    {
        return;
    }

    public async Task<UserDomain?> GetUserByIdAsync(Guid userId)
    {
        var users = madeUpData.Users.Where(user => user.Id== userId);

        return users.FirstOrDefault();
    }

    public async Task<UserDomain[]?> GetUsersBySessionIdAsync(Guid sessionId)
    {
        return null;
    }

}

public class MySessionRepository : ISessionRepository
{
    private readonly MadeUpData madeUpData;
    public MySessionRepository(MadeUpData madeUpData)
    {
        this.madeUpData = madeUpData;
    }

    public async Task AddSessionAsync(Session session)
    {
        madeUpData.Sessions.Add(session);
    }

    public async Task<Session[]?> GetAllSessions()
    {
        return madeUpData.Sessions.ToArray();
    }

    public async Task<Session[]?> GetLastSessionsAsync(int numberOfSessions)
    {
        return madeUpData.Sessions.GetRange(madeUpData.Sessions.Count()-numberOfSessions,numberOfSessions).ToArray();
    }

    public async Task<Session?> GetSessionByDatesAsync(SessionDates sessionDates)
    {
        return madeUpData.Sessions.FirstOrDefault(session=>session.SessionDates==sessionDates);
    }

    public async Task<Session?> GetSessionByIdAsync(Guid sessionId)
    {
        return madeUpData.Sessions.FirstOrDefault(session=>session.Id==sessionId);
    }

    public async Task UpdateSessionAsync(Session session)
    {
        var ses = madeUpData.Sessions.First(ses=>ses.Id==session.Id);
        ses = session;
    }

}

public class MyTableParser : ITableParser
{
    public Task<Result<ChildDto[]>> GetChildrenAsync(string url)
    {
        throw new NotImplementedException();
    }

}

public class MyLogger : ILogger
{
    public async Task Log(long tgId, DateOnly date, bool success)
    {
        return;
    }

}