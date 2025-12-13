using OzshBot.Application.DtoModels;
using OzshBot.Domain.ValueObjects;
using OzshBot.Infrastructure;

namespace Tests.InfrastructureTests.ParserTests;

[TestFixture]
public class ChildInfoParserTests
{
    //потом допишу
    
    [Test]
    public void ChildInfoParser_ParseNameTest()
    {
        Assert.Pass();
    }

    [Test]
    public void ChildInfoParser_Works_WithIncorrectColumnIndexes()
    {
        Assert.Pass();
    }

    [Test]
    public void ChildInfoParser_Fails_WithIncorrectColumnIndexes()
    {
        var dict = new Dictionary<string, int>();
        Assert.That(() => new ChildInfoParser(dict), Throws.InvalidOperationException);
    }
    
    
    static IEnumerable<TestCaseData> IncorrectColumnIndexesTestCases =>
        new[]
        {
            new TestCaseData(new Dictionary<string, int>()),
            new TestCaseData(new Dictionary<string, int?>()
            {
                { "фио", -1 },
            })
        };

    static IEnumerable<TestCaseData> CorrectColumnIndexesTestCases()
    {
        var childDto = new ChildDto
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            Birthday = new DateOnly(2000,1, 1),
            ChildInfo = new ChildInfo
            {
                EducationInfo = new EducationInfo(){ Class = 6, School = "school 1"}
            },
            City = "Muhosransk",
            Email = "child@child.com",
            PhoneNumber = "+71111111111"
        };
        
        return new[] {
            new TestCaseData(
                new List<string>()
            {
                
            }, 
                new Dictionary<string, int>()
            {
                {"фио", 1},
                {"класс", 2},
                {"город", 3},
                {"школа", 4}, 
                {"день рождения", 5},
                {"телефон", 6},
                {"email",  7},
                {"комментарий", 8},
                {"статус заявки на сайте", 9}
            })
        };
    }
    
}