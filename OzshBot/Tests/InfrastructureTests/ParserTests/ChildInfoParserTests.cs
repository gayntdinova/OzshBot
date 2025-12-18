using OzshBot.Application.DtoModels;
using OzshBot.Domain.ValueObjects;
using OzshBot.Infrastructure;
using OzshBot.Infrastructure.Parser;

namespace Tests.InfrastructureTests.ParserTests;

[TestFixture]
public class ChildInfoParserTests
{
    private static readonly Dictionary<string, int> DefautDict = new Dictionary<string, int>()
    {
        { "фио", 0 },
        { "класс", 1 },
        { "город", 2 },
        { "школа", 3 },
        { "день рождения", 4 },
        { "телефон", 5 },
        { "email", 6 },
        { "комментарий", 7 },
        { "статус заявки на сайте", 8 }
    };
    
    [TestCase("Иванов")]
    [TestCase("иван Иванович Ивонов Текст")]
    public void ChildInfoParser_ParseIncorrectNameTest(string name)
    {
        var data = new List<string> { name, "6", "", "", "01.01.2001", "+71111111111", "", "", "" };
        Assert.That(() => new ChildInfoParser(DefautDict).CreateChildDto(data), Throws.ArgumentException);
    }

    [TestCaseSource(nameof(NameTestCases))]
    public void ChildInfoParser_ParseNameTest(Dictionary<string, int> dict, List<string> data, FullName expected)
    {
        var actual = new ChildInfoParser(dict).CreateChildDto(data).FullName;
        Assert.That(actual, Is.EqualTo(expected));
        
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
        Assert.That(actual.ChildInfo.Group, Is.EqualTo(expected.ChildInfo.Group));
    }

    [TestCaseSource(nameof(IncorrectColumnIndexesTestCases)) ]
    public void ChildInfoParser_Fails_WithIncorrectColumnIndexes(Dictionary<string, int> dict)
    {
        Assert.That(() => new ChildInfoParser(dict), Throws.InvalidOperationException);
    }

    [TestCaseSource(nameof(IncorrectDataTestCases))]
    public void ChildInfoParser_Fails_WithIncorrectData(Dictionary<string, int> dict, List<string> data, FullName expected)
    {
        Assert.Pass();
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
                DefautDict,
                childDto),
            new TestCaseData(
                new List<string>()
                {
                    "",
                    "",
                    "child@child.com",
                    "+71111111111",
                    "01.01.2001",
                    "school 1",
                    "екатеринбург",
                    "6",
                    "Иванов Иван Иванович",
                }, 
                new Dictionary<string, int>()
                {
                    {"фио", 8},
                    {"класс", 7},
                    {"город", 6},
                    {"школа", 5}, 
                    {"день рождения", 4},
                    {"телефон", 3},
                    {"email",  2},
                    {"комментарий", 1},
                    {"статус заявки на сайте", 0}
                }, 
                childDto),
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
                    "",
                    "5"
                }, 
                new Dictionary<string, int>()
                {
                    { "фио", 0 },
                    { "класс", 1 },
                    { "город", 2 },
                    { "школа", 3 },
                    { "день рождения", 4 },
                    { "телефон", 5 },
                    { "email", 6 },
                    { "комментарий", 7 },
                    { "статус заявки на сайте", 8 },
                    { "отряд", 9}
                },
                childDto = new ChildDto
                {
                    FullName = new FullName("Иванов", "Иван", "Иванович"),
                    Birthday = new DateOnly(2001,1, 1),
                    ChildInfo = new ChildInfo
                    {
                        EducationInfo = new EducationInfo(){ Class = 6, School = "school 1"},
                        Group = 5
                    },
                    City = "екатеринбург",
                    Email = "child@child.com",
                    PhoneNumber = "+71111111111",
                }),
        };
    }
    
    static IEnumerable<TestCaseData> NameTestCases()
    {
        return new List<TestCaseData>()
        {
            new(DefautDict, new List<string> { "Иванович Иван", "6", "", "", "01.01.2001", "+71111111111", "", "", "" },
                new FullName("Иванович", "Иван")),
            new(DefautDict, new List<string> { "Иванович Иван Иванов", "6", "", "", "01.01.2001", "+71111111111", "",
                    "", "" },
                new FullName("Иванович", "Иван", "Иванов"))
        };
    }

    static IEnumerable<TestCaseData> IncorrectDataTestCases()
    {
        return new List<TestCaseData>();
    }
}