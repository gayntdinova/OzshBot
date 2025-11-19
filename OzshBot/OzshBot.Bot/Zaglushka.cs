using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using System;
using OzshBot.Application.Interfaces;
using OzshBot.Application.Implementations;
using OzshBot.Domain.ValueObjects;
using OzshBot.Domain.Enums;
using Ninject;
using OzshBot.Application.DtoModels;
using OzshBot.Domain.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Formats.Asn1;
using FluentResults;
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

    public List<Child> Children;
    public List<Counsellor> Counsellors;

    public MadeUpData()
    {
        Children = new();
        Counsellors = new();

        var rand = new Random();
        for (int i = 0; i < 100; i++)
        {
            Children.Append(GenerateRandomChild(rand));
            Counsellors.Append(GenerateRandomCounsellor(rand));
        }
    }
    private int id = 0;
    private Child GenerateRandomChild(Random random)
    => new Child
    {
        FullName = GenerateRandomFullName(random),
        Birthday = new DateOnly(),
        Town = Towns[random.Next(Towns.Length)],
        TelegramInfo = new TelegramInfo
        {
            TgId = id++,
            TgUsername = "child" + id.ToString()
        },
        PhoneNumber = $"+79{(random.Next(100000000, 999999999))}",
        Email = $"child{id}@child{id}.com",
        EducationInfo = new EducationInfo
        {
            Class = random.Next(1, 11),
            School = Schools[random.Next(Schools.Length)]
        },
        Group = 1000 + id,
        Sessions = new List<Session> { },
        Parents = GenerateContactPersonList(random)
    };

    private Counsellor GenerateRandomCounsellor(Random random)
    => new Counsellor
    {
        FullName = GenerateRandomFullName(random),
        Birthday = new DateOnly(),
        Town = Towns[random.Next(Towns.Length)],
        TelegramInfo = new TelegramInfo
        {
            TgId = id++,
            TgUsername = "counsellor" + id.ToString()
        },
        PhoneNumber = $"+79{(random.Next(100000000, 999999999))}",
        Email = $"counsellor{id}@counsellor{id}.com",
        Group = 1000 + id,
        Sessions = new List<Session> { }
    };

    private List<ContactPerson> GenerateContactPersonList(Random random)
    {
        var length = random.Next(0, 2);
        var list = new List<ContactPerson>();
        for (int i = 0; i < length; i++)
        {
            list.Add(new ContactPerson
            {
                FullName = GenerateRandomFullName(random),
                Id = new Guid(),
                PhoneNumber = $"+79{(random.Next(100000000, 999999999))}"
            });
        }
        return list;
    }

    private FullName GenerateRandomFullName(Random random)
    => new FullName
    {
        Name = Names[random.Next(Names.Length)],
        Surname = Surnames[random.Next(Names.Length)],
        Patronymic = Patronymics[random.Next(Names.Length)]
    };
}








public class MyEditServise : IEditService
{
    public Task<Result> AddUserAsync(UserDto user)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteUserAsync(UserDto user)
    {
        throw new NotImplementedException();
    }

    public Task<Result> EditUserAsync(UserDto user)
    {
        throw new NotImplementedException();
    }
}

public class MyFindServise : IFindService
{
    private readonly MadeUpData madeUpData;
    public MyFindServise(MadeUpData madeUpData)
    {
        this.madeUpData = madeUpData;
    }
    public Task<UserDto> FindUserByTgUserNameAsync(string tgName)
    {
        throw new NotImplementedException();
    }

    public Task<ManyUsersDto> FindUsersByClassAsync(int classNumber)
    {
        var children = madeUpData.Children.Where(child => child.EducationInfo.Class == classNumber);
        var result = new ManyUsersDto
        {
            Child = children.ToArray(),
            Counsellor = new Counsellor[] { }
        };
        return Task.FromResult(result);
    }

    public Task<ManyUsersDto> FindUsersByFullNameAsync(FullName fullName)
    {
        var children = madeUpData.Children;
        if(fullName.Name!=null)
            children = children.Where(child => child.FullName.Name == fullName.Name).ToList();
        if (fullName.Surname != null)
            children = children.Where(child => child.FullName.Surname == fullName.Surname).ToList();
        if (fullName.Patronymic != null)
            children = children.Where(child => child.FullName.Patronymic == fullName.Patronymic).ToList();

        var counsellors = madeUpData.Counsellors;
        if (fullName.Name != null)
            counsellors = counsellors.Where(counsellor => counsellor.FullName.Name == fullName.Name).ToList();
        if (fullName.Surname != null)
            counsellors = counsellors.Where(counsellor => counsellor.FullName.Surname == fullName.Surname).ToList();
        if (fullName.Patronymic != null)
            counsellors = counsellors.Where(counsellor => counsellor.FullName.Patronymic == fullName.Patronymic).ToList();

        var result = new ManyUsersDto
        {
            Child = children.ToArray(),
            Counsellor = counsellors.ToArray()
        };
        return Task.FromResult(result);
    }

    public Task<ManyUsersDto> FindUsersByGroupAsync(int group)
    {
        var children = madeUpData.Children.Where(child => child.Group == group);
        var counsellors = madeUpData.Counsellors.Where(counsellors => counsellors.Group == group);
        var result = new ManyUsersDto
        {
            Child = children.ToArray(),
            Counsellor = counsellors.ToArray()
        };
        return Task.FromResult(result);
    }

    public Task<ManyUsersDto> FindUsersByTownAsync(string town)
    {
        var children = madeUpData.Children.Where(child => child.Town == town);
        var counsellors = madeUpData.Counsellors.Where(counsellors => counsellors.Town == town);
        var result = new ManyUsersDto
        {
            Child = children.ToArray(),
            Counsellor = counsellors.ToArray()
        };
        return Task.FromResult(result);
    }
}
