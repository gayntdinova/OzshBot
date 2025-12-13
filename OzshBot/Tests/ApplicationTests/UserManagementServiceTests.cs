using FakeItEasy;
using FluentAssertions;
using FluentResults;
using OzshBot.Application;
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
    private ITableParser tableParser;
    private UserManagementService userManagementService;
    private SessionManager sessionManager;
    
    [SetUp]
    public void Setup()
    {
        userRepository = A.Fake<IUserRepository>();
        tableParser = A.Fake<ITableParser>();
        sessionManager = A.Fake<SessionManager>();
        userManagementService = new(userRepository, sessionManager, tableParser);
    }

    [Test]
    public void EditUserAsync_ExistedUser_ReturnOk()
    {
        Assert.Pass();
    }
    
    [Test]
    public void EditUserAsync_NotExistedUser_ReturnFail()
    {
        Assert.Pass();
    }

    [Test]
    public void AddUserAsync_NewChild_ReturnOk()
    {
        Assert.Pass();
    }

    [Test]
    public void AddUserAsync_ExistedChild_ReturnFail()
    {
        Assert.Pass();
    }

    [Test]
    public void DeleteUserAsync_ExistedUser_ReturnOk()
    {
        Assert.Pass();
    }

    [Test]
    public void DeleteUserAsync_NotExistedUser_ReturnFail()
    {
        Assert.Pass();
    }

    [Test]
    public void LoadTableAsync_IncorrectUrl_ReturnFail()
    {
        Assert.Pass();
    }

    [Test]
    public async Task LoadTableAsync_IncorrectRowsInParsing_ReturnFail()
    {
        var url = "";
        A.CallTo(() => tableParser.GetChildrenAsync(url))
            .Returns(Result.Fail(new IncorrectRowError(2)));
        var result = await userManagementService.LoadTableAsync(url);
        result.IsSuccess.Should().BeFalse();
        result.HasError<IncorrectRowError>().Should().BeTrue();
    }

    [Test]
    public async Task LoadTableAsync_HasGroup_ReturnOk()
    {
        var child1 = new ChildDto
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
            ChildInfo = new ChildInfo
            {
                Group = 5,
                Sessions = [],
                ContactPeople = []
            }
        };
        var url = "";
        A.CallTo(() => tableParser.GetChildrenAsync(url))
            .Returns(Result.Ok());
        A.CallTo(() => sessionManager.GetOrCreateSession())
            .Returns(new Session
            {
                Year = 2025,
                Season = Season.Winter
            });
        var result = await userManagementService.LoadTableAsync(url);
        result.IsSuccess.Should().BeTrue();
        
    }
    
    [Test]
    public async Task LoadTableAsync_HasNotGroup_ReturnOk()
    {
        var child1 = new ChildDto
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            PhoneNumber = "+79999999999",
            ChildInfo = new ChildInfo
            {
                Sessions = [],
                ContactPeople = []
            }
        };
        var url = "";
        A.CallTo(() => tableParser.GetChildrenAsync(url))
            .Returns(Result.Ok());
        var result = await userManagementService.LoadTableAsync(url);
        result.IsSuccess.Should().BeTrue();
    }
}