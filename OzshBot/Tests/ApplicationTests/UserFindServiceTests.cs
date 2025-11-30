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
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "@testUser1" };
        var foundUser = new User
        {
            FullName = new FullName("Иванов","Иван"," Иванович"),
            TelegramInfo = telegramInfo,
            Birthday = new DateOnly(1001, 1, 1),
            City = "Екатеринбург",
            PhoneNumber = "+79123456789",
            Email = "ivanov@mail.ru",
            ChildInfo = new ChildInfo
            {
                EducationInfo = new EducationInfo
                {
                    Class = 1,
                    School = "школа 2"
                },
                Group = null,
                Sessions = [],
                ContactPeople = []
            },
            CounsellorInfo = null,
            Role = Role.Child
        };
        
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult(foundUser));
        
        var user = await userFindService.FindUserByTgAsync(telegramInfo);
        
        user.IsSuccess.Should().BeTrue();
        user.Value.Should().BeEquivalentTo(foundUser);
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))
            .MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public async Task FindUserByTgAsync_UnknownUser_ReturnsResultFail()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "@testUser1" };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult<User>(null!));
        
        var user = await userFindService.FindUserByTgAsync(telegramInfo);
        
        user.IsSuccess.Should().BeFalse();
        user.Errors[0].Message.Should().Be("user with @testUser1 was not found");
    }

    [Test]
    public async Task FindUsersByClassAsync_ClassWithChildren_ReturnsResultOk()
    {
        
    }

    [Test]
    public async Task FindUsersByClassAsync_EmptyClass_ReturnsResultFail()
    {
        A.CallTo(() => userRepository.GetUsersByClassAsync(1))!
            .Returns(Task.FromResult<User[]>(null!));
        
        var users = await userFindService.FindUsersByClassAsync(1);
        users.IsSuccess.Should().BeFalse();
        users.Errors[0].Message.Should().Be("users with 1 was not found");
    }

    [Test]
    public async Task FindUsersByGroupAsync_GroupWithChildren_ReturnsResultOk()
    {
        
    }
    
    [Test]
    public async Task FindUsersByGroupAsync_EmptyGroup_ReturnsResultFail()
    {
        A.CallTo(() => userRepository.GetUsersByGroupAsync(0))!
            .Returns(Task.FromResult<User[]>(null!));
        
        var users = await userFindService.FindUsersByGroupAsync(0);
        users.IsSuccess.Should().BeFalse();
        users.Errors[0].Message.Should().Be("users with 0 was not found");
    }

    [Test]
    public async Task FindUsersAsync_ByTelegram_ReturnsResultOk()
    {
        
    }
    
    [Test]
    public async Task FindUsersAsync_ByCity_ReturnsResultOk()
    {
        
    }
    
    [Test]
    public async Task FindUsersAsync_BySchool_ReturnsResultOk()
    {
        
    }
    
    [Test]
    public async Task FindUsersAsync_ByFullName_ReturnsResultOk()
    {
        
    }
    
}