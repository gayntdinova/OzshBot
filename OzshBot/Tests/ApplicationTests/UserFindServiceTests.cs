using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;
using FakeItEasy;
using FluentAssertions;
using FluentResults;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace Tests.ApplicationTests;

[TestFixture]
public class UserFindServiceTests
{
    private IUserRepository userRepository;
    private UserFindService userFindService;
    
    [SetUp]
    public void Setup()
    {
        userRepository = A.Fake<IUserRepository>();
        userFindService = new(userRepository);
    }
    
    [Test]
    public async Task FindUserByTgAsync_KnownUser_ReturnsResultOk()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = new FullName(),
            TelegramInfo = telegramInfo,
            PhoneNumber = "+79999999999",
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult(foundUser));
        
        var result = await userFindService.FindUserByTgAsync(telegramInfo);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(foundUser);
    }
    
    [Test]
    public async Task FindUserByTgAsync_UnknownUser_ReturnsResultFail()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))
            .Returns(Task.FromResult<User?>(null));
        
        var result = await userFindService.FindUserByTgAsync(telegramInfo);
        
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task FindUsersByClassAsync_ClassWithChildren_ReturnsResultOk()
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
            FullName = new FullName(),
            PhoneNumber = "+79999999999"
        };
        A.CallTo(() => userRepository.GetUsersByClassAsync(6))!
            .Returns(Task.FromResult<User[]>([firstChild]));
        
        var result = await userFindService.FindUsersByClassAsync(6);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([firstChild]);
    }

    [Test]
    public async Task FindUsersByClassAsync_EmptyClass_ReturnsResultFail()
    {
        A.CallTo(() => userRepository.GetUsersByClassAsync(1))
            .Returns(Task.FromResult<User[]?>(null));
        
        var result = await userFindService.FindUsersByClassAsync(1);
        
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task FindUsersByGroupAsync_GroupWithChildren_ReturnsResultOk()
    {
        var firstChild = new User
        {
            ChildInfo = new ChildInfo
            {
                Group = 1,
            },
            FullName = new FullName(),
            PhoneNumber = "+79999999999"
        };
        A.CallTo(() => userRepository.GetUsersByGroupAsync(1))!
            .Returns([firstChild]);
        
        var result = await userFindService.FindUsersByGroupAsync(1);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([firstChild]);
    }
    
    [Test]
    public async Task FindUsersByGroupAsync_EmptyGroup_ReturnsResultFail()
    {
        A.CallTo(() => userRepository.GetUsersByGroupAsync(0))!
            .Returns(Task.FromResult<User[]?>(null));
        
        var result = await userFindService.FindUsersByGroupAsync(0);
        
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task FindUsersAsync_ByTelegram_ReturnsResultOk()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = new FullName(),
            TelegramInfo = telegramInfo,
            PhoneNumber = "+79999999999",
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>.That.Matches(t => 
                    t.TgUsername == "testUser1" && 
                    t.TgId == null)))!
            .Returns(Task.FromResult(foundUser));
        
        var result = await userFindService.FindUserAsync("testUser1");
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]);
    }
    
    [Test]
    public async Task FindUsersAsync_ByCity_ReturnsResultOk()
    {
        var foundUser = new User
        {
            FullName = new FullName(),
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
        
        var result = await userFindService.FindUserAsync("Екатеринбург");
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_BySchool_ReturnsResultOk()
    {
        var foundUser = new User
        {
            FullName = new FullName(),
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
        
        var result = await userFindService.FindUserAsync("СУНЦ");
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_BySurnameAndName_ReturnsResultOk()
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
                A<FullName>.That.Matches(fn =>
                    fn.Name == "Иванов" &&
                    fn.Surname == "Иван" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<FullName>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var result = await userFindService.FindUserAsync(input);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_ByNameAndSurname_ReturnsResultOk()
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
                A<FullName>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var result = await userFindService.FindUserAsync(input);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    
    [Test]
    public async Task FindUsersAsync_ByPatronomyc_ReturnsResultOk()
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
                A<FullName>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == null &&
                    fn.Patronymic == "Иванович")))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<FullName>.That.Matches(fn =>
                    fn.Name == "Иванович" &&
                    fn.Surname == null &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<FullName>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == "Иванович" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        
        var result = await userFindService.FindUserAsync(input);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_BySurname_ReturnsResultOk()
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
                A<FullName>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == null &&
                    fn.Patronymic == "Иванов")))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<FullName>.That.Matches(fn =>
                    fn.Name == "Иванов" &&
                    fn.Surname == null &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<FullName>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var result = await userFindService.FindUserAsync(input);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_ByName_ReturnsResultOk()
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
                A<FullName>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == null &&
                    fn.Patronymic == "Иван")))
            .Returns(Task.FromResult<User[]?>(null));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<FullName>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == null &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        A.CallTo(() => userRepository.GetUsersByFullNameAsync(
                A<FullName>.That.Matches(fn =>
                    fn.Name == null &&
                    fn.Surname == "Иван" &&
                    fn.Patronymic == null)))
            .Returns(Task.FromResult<User[]?>(null));
        
        var result = await userFindService.FindUserAsync(input);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_ByFullName_ReturnsResultOk()
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
                A<FullName>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == "Иванович")))
            .Returns(Task.FromResult<User[]?>([foundUser]));
        
        var result = await userFindService.FindUserAsync(input);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }

    [Test]
    public async Task FindUsersAsync_ReturnsResultFail()
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
                A<FullName>.That.Matches(fn =>
                    fn.Name == "Иван" &&
                    fn.Surname == "Иванов" &&
                    fn.Patronymic == "Иванович")))
            .Returns(Task.FromResult<User[]?>(null));
        
        var result = await userFindService.FindUserAsync(input);
        
        result.IsSuccess.Should().BeFalse();
    }
}