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
    public void Test1()
    {
        Assert.Pass();
    }
}