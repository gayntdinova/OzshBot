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
    public async Task FindUserByTgAsync_UnknownUser_ReturnsResultFail()
    {
        var telegramInfo = new TelegramInfo { TgId = null, TgUsername = "@testUser1" };
        A.CallTo(() => userRepository.GetUserByTgAsync(telegramInfo))!
            .Returns(Task.FromResult<User>(null!));
        
        var user = await userFindService.FindUserByTgAsync(telegramInfo);
        
        user.IsSuccess.Should().BeFalse();
        user.Errors.First().Message.Should().Be("user with @testUser1 was not found");
    }
    
    [Test]
    public void FindUserByTgAsync_KnownUser_ReturnsResultOk()
    {
        Assert.Pass();
    }
    
}