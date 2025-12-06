using FakeItEasy;
using OzshBot.Application.RepositoriesInterfaces;
using OzshBot.Application.Services;

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
    public void Test1()
    {
        Assert.Pass();
    }
}