using OzshBot.Infrastructure;

namespace Tests.InfrastructureTests.ParserTests;

[TestFixture]
public class TableParserTests
{
    private static readonly IList<object> DefaultFirstRow = new List<object>()
    {
        "ФИО", "Класс", "Город", "Школа", "День рождения", "Телефон", "Email", "Комментарий",
        "Статус заявки на сайте", "Отряд"
    };
    private static readonly IList<object> DefaultCorrectRow = new List<object>()
    {
        "Иванович Иван Иванов", 6, "екатеринбург", "школа 1", "01.01.2001", "+71111111111", "child@child.com",
        "", "Одобрена", 5
    };
    private static readonly IList<object> IncorrectRow = new List<object>()
    {
        "", 6, "екатеринбург", "школа 1", "01.01.2001", "+71111111111", "child@child.com",
        "", "Одобрена", 5
    };
    private static readonly IList<object?> EmptyRow = new List<object?>()
    {
        null, null, null, null, null, null, null, null, null,
    };


    [TestCaseSource(nameof(CorrectFirstRows))]
    public void ResultIsSuccess_WhenCorrectFirstRow(IList<object> firstRow)
    {
        var table = new List<IList<object>>() { firstRow };
        Assert.That(TableParser.GetChildrenAsync(table).IsSuccess, Is.True);
    }
    
    [TestCaseSource(nameof(CorrectTableData))]
    public void ResultIsSuccess_WhenCorrectData(IList<IList<object>> table, int count)
    {
        Assert.That(TableParser.GetChildrenAsync(table).IsSuccess, Is.True);
    }
    
    [TestCaseSource(nameof(CorrectTableData))]
    public void ScipsEmptyRows_WhenCorrectData(IList<IList<object>> table, int count)
    {
        Assert.That(TableParser.GetChildrenAsync(table).IsSuccess, Is.True);
        var result = TableParser.GetChildrenAsync(table).Value;
        Assert.That(result.Count, Is.EqualTo(count));
    }
    
    [TestCaseSource(nameof(IncorrectData))]
    public void ResultFail_WhenIncorrectData(IList<IList<object>> table)
    {
        Assert.That(TableParser.GetChildrenAsync(table).IsFailed, Is.True);
    }
    
    [TestCaseSource(nameof(IncorrectFirstRows))]
    public void ResultFail_WhenIncorrectFirstRow(IList<object> firstRow)
    {
        var table = new List<IList<object>>() { firstRow };
        Assert.That(TableParser.GetChildrenAsync(table).IsFailed, Is.True);
    }

    [Test]
    public void ResultFail_WhenEmptyTable()
    {
        Assert.That(TableParser.GetChildrenAsync(new List<IList<object>>()).IsFailed, Is.True);
    }

    private static IEnumerable<TestCaseData> IncorrectFirstRows => new[]
    {
        new TestCaseData(new List<object>()),
        new TestCaseData(new List<object>()
        {
            "ФИО", "Класс", "Город", "День рождения", "Телефон", "Email", "Комментарий",
            "Статус заявки на сайте", "Отряд"
        }),
    };

    private static IEnumerable<TestCaseData> CorrectFirstRows => new[]
    {
        new TestCaseData(DefaultFirstRow),
        new TestCaseData(new List<object>()
        {
            "ФИО", "Класс", "Город", "Школа", "День рождения", "Телефон", "Email", "Комментарий",
            "Статус заявки на сайте"
        }),
        new TestCaseData(new List<object>()
        {
            "ФИО", "Класс", "Город", "Школа", "День рождения", "Телефон", "Email", "Комментарий",
            "Статус заявки на сайте", "лишний столбец", "и ещё один"
        })
    };

    private static IEnumerable<TestCaseData> CorrectTableData => new[]
    {
        new TestCaseData(new List<IList<object>>() { DefaultFirstRow, DefaultCorrectRow }, 1),
        new TestCaseData(new List<IList<object>>() { DefaultFirstRow, DefaultCorrectRow, EmptyRow, DefaultCorrectRow }, 2),
        new TestCaseData(new List<IList<object>>() { DefaultFirstRow, new List<object>() }, 0),
    };

    private static IEnumerable<TestCaseData> IncorrectData => new[]
    {
        new TestCaseData(new List<IList<object>>() { DefaultFirstRow, IncorrectRow }),
    };
}