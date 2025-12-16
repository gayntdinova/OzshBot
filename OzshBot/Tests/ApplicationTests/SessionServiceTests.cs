using FakeItEasy;
using FluentAssertions;
using OzshBot.Application.AppErrors;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;
using OzshBot.Domain.Entities;
using OzshBot.Domain.ValueObjects;

namespace Tests.ApplicationTests;

[TestFixture]
public class SessionServiceTests
{
    private ISessionRepository sessionRepository;
    private SessionService sessionService;
    [SetUp]
    public void Setup()
    {
        sessionRepository = A.Fake<ISessionRepository>();
        sessionService = new SessionService(sessionRepository);
    }

    [Test]
    public async Task AddSessionAsync_ExistingSession_ReturnsResultFail()
    {
        var session = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24))
        };
        A.CallTo(() => sessionRepository.GetSessionByDatesAsync(new SessionDates(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24))))
            .Returns(Task.FromResult<Session?>(session));
        
        var result = await sessionService.AddSessionAsync(session);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<SessionAlreadyExistsError>().Should().BeTrue();
    }

    [Test]
    public async Task AddSessionAsync_SessionIntersectsWithOther_ReturnsResultFail()
    {
        var session = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24))
        };
        var session2 = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 6), new DateOnly(2025, 8, 17))
        };
        A.CallTo(() => sessionRepository.GetSessionByDatesAsync(
                A<SessionDates>.That.Matches(d => 
                    d.StartDate == new DateOnly(2025, 8, 10) && 
                    d.EndDate == new DateOnly(2025, 8, 24))))
            .Returns(Task.FromResult<Session?>(null));
        A.CallTo(() => sessionRepository.GetAllSessions())
            .Returns(Task.FromResult<Session[]?>([session2]));
        
        var result = await sessionService.AddSessionAsync(session);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<SessionIntersectError>().Should().BeTrue();
    }

    [Test]
    public async Task AddSessionAsync_ReturnsResultOk()
    {
        var session = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24))
        };
        var session2 = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 25), new DateOnly(2025, 9, 17))
        };
        A.CallTo(() => sessionRepository.GetSessionByDatesAsync(
                A<SessionDates>.That.Matches(d => 
                    d.StartDate == new DateOnly(2025, 8, 10) && 
                    d.EndDate == new DateOnly(2025, 8, 24))))
            .Returns(Task.FromResult<Session?>(null));
        A.CallTo(() => sessionRepository.GetAllSessions())
            .Returns(Task.FromResult<Session[]?>([session2]));
        
        var result = await sessionService.AddSessionAsync(session);
        
        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task EditSessionAsync_NotExistingSession_ReturnsResultFail()
    {
        var session = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24))
        };
        A.CallTo(() => sessionRepository.GetSessionByIdAsync(session.Id))
            .Returns(Task.FromResult<Session?>(null));
        
        var result = await sessionService.EditSessionAsync(session);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<SessionNotFoundError>().Should().BeTrue();
    }

    [Test]
    public async Task EditSessionAsync_SessionIntersectsWithOther_ReturnsResultFail()
    {
        var session = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24))
        };
        var session2 = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 6), new DateOnly(2025, 8, 17))
        };
        A.CallTo(() => sessionRepository.GetSessionByIdAsync(session.Id))
            .Returns(Task.FromResult<Session?>(session));
        A.CallTo(() => sessionRepository.GetAllSessions())
            .Returns(Task.FromResult<Session[]?>([session, session2]));
        
        var result = await sessionService.EditSessionAsync(session);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<SessionIntersectError>().Should().BeTrue();
    }

    [Test]
    public async Task EditSessionAsync_ReturnsResultOk()
    {
        var session = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24))
        };
        var session2 = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 25), new DateOnly(2025, 9, 17))
        };
        A.CallTo(() => sessionRepository.GetSessionByIdAsync(session.Id))
            .Returns(Task.FromResult<Session?>(session));
        A.CallTo(() => sessionRepository.GetAllSessions())
            .Returns(Task.FromResult<Session[]?>([session, session2]));
        
        var result = await sessionService.EditSessionAsync(session);
        
        result.IsSuccess.Should().BeTrue();
    }
    
    [Test]
    public async Task GetAllSessions_NoSessions_ReturnsEmptyArray()
    {
        A.CallTo(() => sessionRepository.GetAllSessions())
            .Returns(Task.FromResult<Session[]?>(null));

        var sessions = await sessionService.GetAllSessionsAsync();
        
        sessions.Should().BeEmpty();
    }
    
    [Test]
    public async Task GetAllSessionsAsync()
    {
        var session = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24))
        };
        var session2 = new Session
        {
            SessionDates = new(new DateOnly(2025, 8, 25), new DateOnly(2025, 9, 17))
        };
        A.CallTo(() => sessionRepository.GetAllSessions())
            .Returns(Task.FromResult<Session[]?>([session, session2]));

        var sessions = await sessionService.GetAllSessionsAsync();
        
        sessions.Should().BeEquivalentTo([session, session2]);
    }
}