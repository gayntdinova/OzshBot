using FakeItEasy;
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

    }

    [Test]
    public async Task AddSessionAsync_SessionIntersectsWithOther_ReturnsResultFail()
    {
        
    }

    [Test]
    public async Task AddSessionAsync_ReturnsResultOk()
    {
        
    }
    
    [Test]
    public async Task EditSessionAsync_NotExistingSession_ReturnsResultFail()
    {
        
    }

    [Test]
    public async Task EditSessionAsync_SessionIntersectsWithOther_ReturnsResultFail()
    {
        
    }

    [Test]
    public async Task EditSessionAsync_ReturnsResultOk()
    {
        
    }
    
    [Test]
    public async Task GetLastSessions_NoSessions_ReturnsResultFail()
    {
        
    }
    
    [Test]
    public async Task GetLastSessionsAsync_ReturnsResultOk()
    {
        
    }
}