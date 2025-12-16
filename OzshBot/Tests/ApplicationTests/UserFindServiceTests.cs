using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;
using FakeItEasy;
using FluentAssertions;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace Tests.ApplicationTests;

[TestFixture]
public class UserFindServiceTests
{
    private IUserRepository userRepository;
    private ISessionRepository sessionRepository;
    private UserFindService userFindService;
    
    [SetUp]
    public void Setup()
    {
        userRepository = A.Fake<IUserRepository>();
        sessionRepository = A.Fake<ISessionRepository>();
        userFindService = new(userRepository, sessionRepository);
    }
    
    [Test]
    public async Task FindUserByTgAsync_KnownUser()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            TelegramInfo = telegramInfo,
            PhoneNumber = "+79999999999",
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult(foundUser));
        
        var user = await userFindService.FindUserByTgAsync(telegramInfo);
        
        user.Should().BeEquivalentTo(foundUser);
    }
    
    [Test]
    public async Task FindUserByTgAsync_UnknownUser_ReturnsNull()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))
            .Returns(Task.FromResult<User?>(null));
        
        var user = await userFindService.FindUserByTgAsync(telegramInfo);

        user.Should().BeNull();
    }

    [Test]
    public async Task FindUsersByClassAsync_ReturnChildrenWithInputClassAndGroup()
    {
        var firstChild = new User
        {
            ChildInfo = new ChildInfo
            {
                EducationInfo = new EducationInfo
                {
                    Class = 6,
                    School = "школа 2"
                },
            },
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
            Role = Role.Child
        };
        
        var secondChild = new User
        {
            ChildInfo = new ChildInfo
            {
                EducationInfo = new EducationInfo
                {
                    Class = 6,
                    School = "школа 2"
                },
                Group = 2
            },
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
            Role = Role.Child
        };
        A.CallTo(() => userRepository.GetUsersByClassAsync(6))!
            .Returns(Task.FromResult<User[]>([firstChild, secondChild]));
        
        var users = await userFindService.FindUsersByClassAsync(6);
        
        users.Should().BeEquivalentTo([secondChild]);
    }

    [Test]
    public async Task FindUsersByClassAsync_EmptyClass_ReturnsEmptyArray()
    {
        A.CallTo(() => userRepository.GetUsersByClassAsync(1))
            .Returns(Task.FromResult<User[]?>(null));
        
        var users = await userFindService.FindUsersByClassAsync(1);

        users.Should().BeEmpty();
    }

    [Test]
    public async Task FindUsersByGroupAsync_GroupWithChildren()
    {
        var firstChild = new User
        {
            ChildInfo = new ChildInfo
            {
                Group = 1,
            },
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999"
        };
        A.CallTo(() => userRepository.GetUsersByGroupAsync(1))!
            .Returns([firstChild]);
        
        var users = await userFindService.FindUsersByGroupAsync(1);
        
        users.Should().BeEquivalentTo([firstChild]);
    }
    
    [Test]
    public async Task FindUsersByGroupAsync_EmptyGroup_ReturnsEmptyArray()
    {
        A.CallTo(() => userRepository.GetUsersByGroupAsync(0))!
            .Returns(Task.FromResult<User[]?>(null));
        
        var users = await userFindService.FindUsersByGroupAsync(0);
        
        users.Should().BeEmpty();
    }

    #region FindUsersAsyncTests
    [Test]
    public async Task FindUsersAsync_ByTelegram()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            TelegramInfo = telegramInfo,
            PhoneNumber = "+79999999999",
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>.That.Matches(t => 
                    t.TgUsername == "testUser1" && 
                    t.TgId == null)))!
            .Returns(Task.FromResult(foundUser));
        
        var users = await userFindService.FindUserAsync("testUser1");
        
        users.Should().BeEquivalentTo([foundUser]);
    }
    
    [Test]
    public async Task FindUsersAsync_ByCity()
    {
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            City = "Екатеринбург",
            PhoneNumber = "+79999999999",
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>.That.Matches(t => 
                    t.TgUsername == "Екатеринбург" && 
                    t.TgId == null)))!
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync("Екатеринбург"))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var users = await userFindService.FindUserAsync("Екатеринбург");
        
        users.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_BySchool()
    {
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            ChildInfo = new ChildInfo
            {
                EducationInfo = new()
                {
                    Class = 0,
                    School = "СУНЦ"
                }
            },
            PhoneNumber = "+79999999999"
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>.That.Matches(t => 
                    t.TgUsername == "СУНЦ" && 
                    t.TgId == null)))!
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync("СУНЦ"))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync("СУНЦ"))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var users = await userFindService.FindUserAsync("СУНЦ");
        
        users.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_BySurnameAndName()
    {
        var input = "Иванов Иван";
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999"
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>._))
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == "Иванов" &&
                    fn.Surname == "Иван" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var users = await userFindService.FindUserAsync(input);
        
        users.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_ByNameAndSurname()
    {
        var input = "Иван Иванов";
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999"
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>._))
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var users = await userFindService.FindUserAsync(input);
        
        users.Should().BeEquivalentTo([foundUser]); 
    }
    
    
    [Test]
    public async Task FindUsersAsync_ByPatronomyc()
    {
        var input = "Иванович";
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999"
        };
        
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>._))
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == null &&
                    fn.Patronymic == "Иванович")))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == "Иванович" &&
                    fn.Surname == null &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == "Иванович" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        
        var users = await userFindService.FindUserAsync(input);
        
        users.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_BySurname()
    {
        var input = "Иванов";
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999"
        };
        
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>._))
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == null &&
                    fn.Patronymic == "Иванов")))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == "Иванов" &&
                    fn.Surname == null &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var users = await userFindService.FindUserAsync(input);
        
        users.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_ByName()
    {
        var input = "Иван";
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999"
        };
        
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>._))
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == null &&
                    fn.Patronymic == "Иван")))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == null &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == "Иван" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        
        var users = await userFindService.FindUserAsync(input);
        
        users.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_ByFullName()
    {
        var input = "Иванов Иван Иванович";
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999"
        };
        
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>._))
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == "Иванович")))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var users = await userFindService.FindUserAsync(input);
        
        users.Should().BeEquivalentTo([foundUser]); 
    }

    [Test]
    public async Task FindUsersAsync_ReturnsEmptyArray()
    {
        var input = "Иванов Иван Иванович";
        var foundUser = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999"
        };
        
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>._))
            .Returns(Task.FromResult<User?>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync(input))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<NameSearch>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == "Иванович")))
            .Returns(Task.FromResult<User[]?>(null));
        
        var users = await userFindService.FindUserAsync(input);

        users.Should().BeEmpty();

    }
    #endregion
}