using FakeItEasy;
using FluentAssertions;
using OzshBot.Application;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace Tests.ApplicationTests;

[TestFixture]
public class SessionServiceTests
{
    private SessionService sessionService;
    private ISessionRepository sessionRepository;
    private IUserRepository userRepository;
    
    [SetUp]
    public void Setup()
    {
        sessionRepository = A.Fake<ISessionRepository>();
        userRepository = A.Fake<IUserRepository>();
        sessionService = new SessionService(sessionRepository, userRepository);
    }
    
    [Test]
    public async Task GetOrCreateSessionAsync_SessionNotExists_NewSession()
    {
        A.CallTo(() => sessionRepository.GetSessionBySeasonAndYearAsync(Season.Winter, 2025))
            .Returns(Task.FromResult<Session?>(null));
        A.CallTo(() => sessionRepository.GetSessionBySeasonAndYearAsync(Season.Autumn, 2025))
            .Returns(Task.FromResult<Session?>(null));
        A.CallTo(() => sessionRepository.AddSessionAsync(A<Session>._))
            .Returns(Task.CompletedTask);
        
        var session = await sessionService.GetOrCreateSessionAsync();
        
        session.Year.Should().Be(2025);
        session.Season.Should().Be(Season.Winter);
    }
    

    [Test]
    public async Task GetOrCreateSessionAsync_WhenLastSessionHasParticipants_ShouldClearGroupsAndUpdate()
    {
        var year = 2025;
        var season = Season.Winter;
        var lastSession = new Session { Year = 2025, Season = Season.Autumn };;
        
        var childWithGroup = new User
        {
            Role = Role.Child,
            ChildInfo = new ChildInfo
            {
                Group = 5,
                Sessions = []
            },
            FullName = new FullName("Петров", "Петр"),
            PhoneNumber = "+79999999999"
        };
        
        var counsellorWithGroup = new User
        {
            Role = Role.Counsellor,
            CounsellorInfo = new CounsellorInfo
            {
                Group = 5,
                Sessions = []
            },
            FullName = new FullName("Петров", "Андрей"),
            PhoneNumber = "+79999999998"
        };

        User[] participants = [childWithGroup, counsellorWithGroup];

        A.CallTo(() => sessionRepository.GetSessionBySeasonAndYearAsync(season, year))
            .Returns(Task.FromResult<Session?>(null));
        A.CallTo(() => sessionRepository.GetSessionBySeasonAndYearAsync(Season.Autumn, 2025))
            .Returns(Task.FromResult<Session?>(lastSession));
        A.CallTo(() => sessionRepository.AddSessionAsync(A<Session>._))
            .Returns(Task.CompletedTask);
        A.CallTo(() => userRepository.GetUsersBySessionIdAsync(lastSession.Id))
            .Returns(Task.FromResult<User[]?>(participants));
        
        var result = await sessionService.GetOrCreateSessionAsync();
        
        result.Should().NotBeNull();
        childWithGroup.ChildInfo.Group.Should().BeNull();
        counsellorWithGroup.CounsellorInfo.Group.Should().BeNull();
    }

    [Test]
    public async Task GetOrCreateSessionAsync_ExistingSession_ReturnsSession()
    {
        var expectedSession = new Session
        {
            Year = 2025,
            Season = Season.Winter
        };
        
        A.CallTo(() => sessionRepository.GetSessionBySeasonAndYearAsync(Season.Winter, 2025))
            .Returns(Task.FromResult<Session?>(expectedSession));
        
        var session = await sessionService.GetOrCreateSessionAsync();
        
        session.Should().BeEquivalentTo(expectedSession);
        
    }
    
}