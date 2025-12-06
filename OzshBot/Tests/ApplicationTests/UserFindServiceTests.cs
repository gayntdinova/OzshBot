using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;
using FakeItEasy;
using FluentAssertions;
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
            FullName = null,
            TelegramInfo = telegramInfo,
            PhoneNumber = null,
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
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult<User>(null!));
        
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
            FullName = null,
            TelegramInfo = null,
            PhoneNumber = null
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
        A.CallTo(() => userRepository.GetUsersByClassAsync(1))!
            .Returns(Task.FromResult<User[]>(null));
        
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
            FullName = null,
            TelegramInfo = null,
            PhoneNumber = null
        };
        A.CallTo(() => userRepository.GetUsersByGroupAsync(1))!
            .Returns(Task.FromResult<User[]>([firstChild]));
        
        var result = await userFindService.FindUsersByGroupAsync(1);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([firstChild]);
    }
    
    [Test]
    public async Task FindUsersByGroupAsync_EmptyGroup_ReturnsResultFail()
    {
        A.CallTo(() => userRepository.GetUsersByGroupAsync(0))!
            .Returns(Task.FromResult<User[]>(null));
        
        var result = await userFindService.FindUsersByGroupAsync(0);
        
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task FindUsersAsync_ByTelegram_ReturnsResultOk()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = null,
            TelegramInfo = telegramInfo,
            PhoneNumber = null,
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
            FullName = null,
            City = "Екатеринбург",
            TelegramInfo = null,
            PhoneNumber = null,
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>.That.Matches(t => 
                    t.TgUsername == "Екатеринбург" && 
                    t.TgId == null)))!
            .Returns(Task.FromResult<User>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync("Екатеринбург"))
            .Returns(Task.FromResult<User[]>([foundUser]));
        
        var result = await userFindService.FindUserAsync("Екатеринбург");
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_BySchool_ReturnsResultOk()
    {
        var foundUser = new User
        {
            FullName = null,
            ChildInfo = new ChildInfo()
            {
                EducationInfo = new()
                {
                    Class = 0,
                    School = "СУНЦ"
                }
            },
            TelegramInfo = null,
            PhoneNumber = null
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(
                A<TelegramInfo>.That.Matches(t => 
                    t.TgUsername == "СУНЦ" && 
                    t.TgId == null)))!
            .Returns(Task.FromResult<User>(null));
        A.CallTo(() => userRepository.GetUsersByCityAsync("СУНЦ"))
            .Returns(Task.FromResult<User[]>(null));
        A.CallTo(() => userRepository.GetUsersBySchoolAsync("СУНЦ"))
            .Returns(Task.FromResult<User[]>([foundUser]));
        
        var result = await userFindService.FindUserAsync("СУНЦ");
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo([foundUser]); 
    }
    
    [Test]
    public async Task FindUsersAsync_ByFullName_ReturnsResultOk()
    {
        //todo
    }
}