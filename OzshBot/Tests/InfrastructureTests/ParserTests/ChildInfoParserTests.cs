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

    [TestCaseSource(nameof(CorrectColumnIndexesTestCases))]
    public void ChildInfoParser_Works_WithCorrectColumnIndexes(List<string> data, Dictionary<string, int> dict, ChildDto expected)
    {
        var actual = new ChildInfoParser(dict).CreateChildDto(data);
        Assert.That(actual.City, Is.EqualTo(expected.City));
        Assert.That(actual.Birthday, Is.EqualTo(expected.Birthday));
        Assert.That(actual.PhoneNumber, Is.EqualTo(expected.PhoneNumber));
        Assert.That(actual.Email,Is.EqualTo(expected.Email));
        Assert.That(actual.ChildInfo.EducationInfo, Is.EqualTo(expected.ChildInfo.EducationInfo));
        //Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(IncorrectColumnIndexesTestCases)) ]
    public void ChildInfoParser_Fails_WithIncorrectColumnIndexes(Dictionary<string, int> dict)
    {
        Assert.That(() => new ChildInfoParser(dict), Throws.InvalidOperationException);
    }
    
    
    static IEnumerable<TestCaseData> IncorrectColumnIndexesTestCases =>
        new[]
        {
            new TestCaseData(new Dictionary<string, int>()),
            new TestCaseData(new Dictionary<string, int>()
            {
                { "фио", -1 },
            })
        };

    static IEnumerable<TestCaseData> CorrectColumnIndexesTestCases()
    {
        var childDto = new ChildDto
        {
            FullName = new FullName("Иванов", "Иван", "Иванович"),
            Birthday = new DateOnly(2001,1, 1),
            ChildInfo = new ChildInfo
            {
                EducationInfo = new EducationInfo(){ Class = 6, School = "school 1"}
            },
            City = "екатеринбург",
            Email = "child@child.com",
            PhoneNumber = "+71111111111"
        };
        
        return new[] {
            new TestCaseData(
                new List<string>()
            {
                "Иванов Иван Иванович",
                "6",
                "екатеринбург",
                "school 1",
                "01.01.2001",
                "+71111111111",
                "child@child.com",
                "",
                ""
            }, 
                new Dictionary<string, int>()
            {
                {"фио", 0},
                {"класс", 1},
                {"город", 2},
                {"школа", 3}, 
                {"день рождения", 4},
                {"телефон", 5},
                {"email",  6},
                {"комментарий", 7},
                {"статус заявки на сайте", 8}
            }, 
                childDto)
        };
    }
    
}