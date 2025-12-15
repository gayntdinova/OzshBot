using OzshBot.Infrastructure;
using OzshBot.Infrastructure.Parser;

namespace Tests.InfrastructureTests.ParserTests;

[TestFixture]
public class GoogleDocsReaderTests
{
    private IList<IList<object>> table = new List<IList<object>>()
    {
        new List<object>()
        {
            "ФИО", "Класс", "Город", "Школа", "День рождения", "Телефон", "Email", "Комментарий",
            "Статус заявки на сайте", "Отряд"
        },
        new List<object>()
        {
            "Иванов Иван Иванович", "6", "екатеринбург", "школа 1", "01.01.2001", "8 (111) 111-11-11", "child@child.com",
            "", "Одобрена", "5"
        }
    };
    
    [Test]
    public async Task GoogleDocsParser_Parse_Test()
    {
        var url =
            "https://docs.google.com/spreadsheets/d/1Nq-eGE8isv5rvIYBqqWay4GVoNfBzC5ogxFuQ5pZicQ/edit?gid=0#gid=0";
        var sheet = await new GoogleDocsReader(url).ReadGoogleSheet();
        Assert.IsNotNull(sheet);
        Assert.That(sheet, Is.EquivalentTo(table));
    }
}