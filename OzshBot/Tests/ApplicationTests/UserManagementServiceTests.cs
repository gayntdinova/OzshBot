using FakeItEasy;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;

namespace Tests.ApplicationTests;

[TestFixture]
public class UserManagementServiceTests
{
    private IUserRepository userRepository;
    private ITableParser tableParser;
    private UserManagementService userManagementService;
    
    [SetUp]
    public void Setup()
    {
        userRepository = A.Fake<IUserRepository>();
        tableParser = A.Fake<ITableParser>();
        userManagementService = new(userRepository, tableParser);
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
    public void LoadTableAsync_IncorrectRowsInParsing_ReturnFail()
    {
        Assert.Pass();
    }

    [Test]
    public void LoadTableAsync_ReturnOk()
    {
        Assert.Pass();
    }
}