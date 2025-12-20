using FakeItEasy;
using FluentAssertions;
using FluentResults;
using OzshBot.Application.AppErrors;
using OzshBot.Application.DtoModels;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;
using OzshBot.Application.ToolsInterfaces;
using OzshBot.Domain.Entities;
using OzshBot.Domain.Enums;
using OzshBot.Domain.ValueObjects;

namespace Tests.ApplicationTests;

[TestFixture]
public class UserManagementServiceTests
{
    private IUserRepository userRepository;
    private ISessionRepository sessionRepository;
    private ITableParser tableParser;
    private UserManagementService userManagementService;
    
    [SetUp]
    public void Setup()
    {
        userRepository = A.Fake<IUserRepository>();
        sessionRepository = A.Fake<ISessionRepository>();
        tableParser = A.Fake<ITableParser>();
        userManagementService = new UserManagementService(userRepository, sessionRepository, tableParser);
    }

    [Test]
    public async Task AddUserAsync_ExistingUser_ReturnsResultFail()
    {
        var child = new ChildDto
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
            ChildInfo = new ChildInfo()
        };
        var user = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
        };
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync(user.PhoneNumber))
            .Returns(Task.FromResult<User?>(user));
        
        var result = await userManagementService.AddUserAsync(child);

        result.IsSuccess.Should().BeFalse();
        result.HasError<UserAlreadyExistsError>().Should().BeTrue();
    }

    [Test]
    public async Task AddUserAsync_ReturnsResultOk()
    {
        var child = new ChildDto
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
            ChildInfo = new ChildInfo()
        };
        var user = new User
        {
            Id = child.Id,
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
            ChildInfo = child.ChildInfo,
            Role = Role.Child
        };
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync(user.PhoneNumber))
            .Returns(Task.FromResult<User?>(null));
        
        var result = await userManagementService.AddUserAsync(child);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(user);
    }

    [Test]
    public async Task EditUserAsync_NotExistingUser_ReturnsResultFail()
    {
        var user = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
        };
        A.CallTo(() => userRepository.GetUserByIdAsync(user.Id))
            .Returns(Task.FromResult<User?>(null));
        
        var result = await userManagementService.EditUserAsync(user);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<UserNotFoundError>().Should().BeTrue();
    }

    [Test]
    public async Task EditUserAsync_ReturnsResultOk()
    {
        var user = new User
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
        };
        
        var editedUser = new User
        {
            Id = user.Id,
            FullName = new FullName("Иванов", "Федор", "Иванович"),
            PhoneNumber = "+79999999989",
        };
        A.CallTo(() => userRepository.GetUserByIdAsync(user.Id))
            .Returns(Task.FromResult<User?>(user));
        
        var result = await userManagementService.EditUserAsync(editedUser);
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(editedUser);
    }

    [Test]
    public async Task DeleteUserAsync_NotExistingUser_ReturnsResultFail()
    {
        var phoneNumber = "+79999999999";
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync(phoneNumber))
            .Returns(Task.FromResult<User?>(null));
        
        var result = await userManagementService.DeleteUserAsync(phoneNumber);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<UserNotFoundError>().Should().BeTrue();
    }

    [Test]
    public async Task DeleteUserAsync_ReturnsResultOk()
    {
        var phoneNumber = "+79999999999";
        var user = new User
        {
            FullName = new FullName("Иванов", "Федор", "Иванович"),
            PhoneNumber = "+79999999989",
        };
        
        A.CallTo(() => userRepository.GetUserByPhoneNumberAsync(phoneNumber))
            .Returns(Task.FromResult<User?>(user));
        
        var result = await userManagementService.DeleteUserAsync(phoneNumber);
        
        result.IsSuccess.Should().BeTrue();
        A.CallTo(() => userRepository.DeleteUserAsync(phoneNumber))
            .MustHaveHappenedOnceExactly();
    }

    [Test]
    public async Task LoadTableAsync_NoSession_ReturnsResultFail()
    {
        var sessionDates = new SessionDates(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24));
        A.CallTo(() => sessionRepository.GetSessionByDatesAsync(sessionDates))
            .Returns(Task.FromResult<Session?>(null));
        
        var result = await userManagementService.LoadTableAsync("", sessionDates);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<SessionNotFoundError>().Should().BeTrue();
    }

    [Test]
    public async Task LoadTableAsync_IncorrectUrl_ReturnsResultFail()
    {
        var sessionDates = new SessionDates(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24));
        var session = new Session
        {
            SessionDates = sessionDates
        };
        A.CallTo(() => sessionRepository.GetSessionByDatesAsync(sessionDates))
            .Returns(Task.FromResult<Session?>(session));
        A.CallTo(() => tableParser.GetChildrenAsync(""))
            .Returns(Task.FromResult(Result.Fail<ChildDto[]>(new InvalidUrlError())));
        
        var result = await userManagementService.LoadTableAsync("", sessionDates);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<InvalidUrlError>().Should().BeTrue();
    }

    [Test]
    public async Task LoadTableAsync_IncorrectRow_ReturnsResultFail()
    {
        var sessionDates = new SessionDates(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24));
        var session = new Session
        {
            SessionDates = sessionDates
        };
        A.CallTo(() => sessionRepository.GetSessionByDatesAsync(sessionDates))
            .Returns(Task.FromResult<Session?>(session));
        A.CallTo(() => tableParser.GetChildrenAsync(""))
            .Returns(Task.FromResult(Result.Fail<ChildDto[]>(new InvalidRowError(5))));
        
        var result = await userManagementService.LoadTableAsync("", sessionDates);
        
        result.IsSuccess.Should().BeFalse();
        result.HasError<InvalidRowError>().Should().BeTrue();
    }
    
    [Test]
    public async Task LoadTableAsync_ChildrenHaveSamePhones_ReturnsResultFail()
    {
        var sessionDates = new SessionDates(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24));
        var session = new Session
        {
            SessionDates = sessionDates
        };
        var children = new ChildDto[]
        {
            new()
            {
                FullName = new FullName("Иванов", "Иван", "Иванович"),
                PhoneNumber = "+79999999999",
                ChildInfo = new ChildInfo
                {
                    Sessions = []
                }
            },
            new()
            {
                FullName = new FullName("Иванов", "Федор", "Иванович"),
                PhoneNumber = "+79999999999",
                ChildInfo = new ChildInfo
                {
                    Sessions = []
                }
            }
        };
        A.CallTo(() => sessionRepository.GetSessionByDatesAsync(sessionDates))
            .Returns(Task.FromResult<Session?>(session));
        A.CallTo(() => tableParser.GetChildrenAsync(""))
            .Returns(Task.FromResult(Result.Ok(children)));
        var capturedUsers = new List<User>();
        A.CallTo(() => userRepository.UpdateUserAsync(A<User>._))
            .Invokes((User user) => capturedUsers.Add(user))
            .Returns(Task.CompletedTask);
        
        var result = await userManagementService.LoadTableAsync("", sessionDates);

        result.IsSuccess.Should().BeFalse();
    }
    
    [Test]
    public async Task LoadTableAsync_ReturnsResultOk()
    {
        var sessionDates = new SessionDates(new DateOnly(2025, 8, 10), new DateOnly(2025, 8, 24));
        var session = new Session
        {
            SessionDates = sessionDates
        };
        var children = new ChildDto[]
        {
            new()
            {
                FullName = new FullName("Иванов", "Иван", "Иванович"),
                PhoneNumber = "+79999999989",
                ChildInfo = new ChildInfo
                {
                    Sessions = []
                }
            },
            new()
            {
                FullName = new FullName("Иванов", "Федор", "Иванович"),
                PhoneNumber = "+79999999909",
                ChildInfo = new ChildInfo
                {
                    Sessions = []
                }
            }
        };
        A.CallTo(() => sessionRepository.GetSessionByDatesAsync(sessionDates))
            .Returns(Task.FromResult<Session?>(session));
        A.CallTo(() => tableParser.GetChildrenAsync(""))
            .Returns(Task.FromResult(Result.Ok(children)));
        var capturedUsers = new List<User>();
        A.CallTo(() => userRepository.UpdateUserAsync(A<User>._))
            .Invokes((User user) => capturedUsers.Add(user))
            .Returns(Task.CompletedTask);
        
        var result = await userManagementService.LoadTableAsync("", sessionDates);
        
        result.IsSuccess.Should().BeTrue();
        foreach (var user in capturedUsers)
        {
            user.ChildInfo.Sessions.Should().Contain(session);
        }
    }
}