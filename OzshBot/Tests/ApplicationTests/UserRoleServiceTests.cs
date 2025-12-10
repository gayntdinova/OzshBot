using FakeItEasy;
using FluentAssertions;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace Tests.ApplicationTests;

[TestFixture]
public class UserRoleServiceTests
{
    private IUserRepository userRepository;
    private UserRoleService userRoleService;
    
    [SetUp]
    public void Setup()
    {
        userRepository = A.Fake<IUserRepository>();
        userRoleService = new(userRepository);
    }

    [Test]
    public async Task GetUserRoleByTgAsync_ReturnsChild()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = null,
            TelegramInfo = telegramInfo,
            PhoneNumber = null,
            Role = Role.Child
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult(foundUser));
        
        var role = await userRoleService.GetUserRoleByTgAsync(telegramInfo);
        
        Assert.That(role, Is.EqualTo(Role.Child));
    }
    
    [Test]
    public async Task GetUserRoleByTgAsync_ReturnsCounsellor()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = null,
            TelegramInfo = telegramInfo,
            PhoneNumber = null,
            Role = Role.Counsellor
        };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult(foundUser));
        
        var role = await userRoleService.GetUserRoleByTgAsync(telegramInfo);
        
        Assert.That(role, Is.EqualTo(Role.Counsellor));
    }
    
    [Test]
    public async Task GetUserRoleByTgAsync_ReturnsUnknown()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult<User?>(null));
        
        var role = await userRoleService.GetUserRoleByTgAsync(telegramInfo);
        
        Assert.That(role, Is.EqualTo(Role.Unknown));
    }
    
    [Test]
    public async Task ActivateUserByPhoneNumberAsync_ReturnsChild()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = null,
            PhoneNumber = "+79999999999",
            TelegramInfo = telegramInfo,
            Role = Role.Child
        };
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync("+79999999999"))!
            .Returns(Task.FromResult(foundUser));
        
        var role = await userRoleService.ActivateUserByPhoneNumberAsync("+79999999999", telegramInfo);
        
        Assert.That(role, Is.EqualTo(Role.Child));
    }
    
    [Test]
    public async Task ActivateUserByPhoneNumberAsync_ReturnsCounsellor()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        var foundUser = new User
        {
            FullName = null,
            PhoneNumber = "+79999999999",
            TelegramInfo = telegramInfo,
            Role = Role.Counsellor
        };
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync("+79999999999"))!
            .Returns(Task.FromResult(foundUser));
        
        var role = await userRoleService.ActivateUserByPhoneNumberAsync("+79999999999", telegramInfo);
        
        Assert.That(role, Is.EqualTo(Role.Counsellor));
    }
    
    [Test]
    public async Task ActivateUserByPhoneNumberAsync_ReturnsUnknown()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "testUser1" };
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync("+79999999999"))!
            .Returns(Task.FromResult<User?>(null));
        
        var role = await userRoleService.ActivateUserByPhoneNumberAsync("+79999999999", telegramInfo);
        
        Assert.That(role, Is.EqualTo(Role.Unknown));
    }
    
    [Test]
    public async Task PromoteToCounsellorAsync_ReturnsResultFail()
    {
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync("+79999999999"))!
            .Returns(Task.FromResult<User?>(null));
        
        var result = await userRoleService.PromoteToCounsellorAsync("+79999999999");

        result.IsSuccess.Should().BeFalse();
    }
    
    [Test]
    public async Task PromoteToCounsellorAsync_ReturnsResultOk()
    {
        var foundUser = new User
        {
            FullName = null,
            PhoneNumber = "+79999999999",
            Role = Role.Child
        };
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync("+79999999999"))!
            .Returns(Task.FromResult(foundUser));
        
        var result = await userRoleService.PromoteToCounsellorAsync("+79999999999");
        
        var expectedUser = new User
        {
            Id = foundUser.Id,
            FullName = null,
            PhoneNumber = "+79999999999",
            Role = Role.Counsellor,
            CounsellorInfo = new CounsellorInfo
            {
                Group = null,
                Sessions = []
            }
        };

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedUser);
    }
}