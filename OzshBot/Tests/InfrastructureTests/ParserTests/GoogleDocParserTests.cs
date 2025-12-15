using OzshBot.Infrastructure.Parser;

namespace Tests.InfrastructureTests.ParserTests;

[TestFixture]
public class GoogleDocParserTests
{
    [Test]
    public async Task GoogleDocsParser_Fails_OnIncorrectUrl()
    {
        var url = "https://www.google.com/";
        var result = await new GoogleDocParser().GetChildrenAsync(url);
        Assert.That(result.IsFailed, Is.True);
    }
    
    [Test]
    public async Task GoogleDocsParser_Sucsess_OnCorrectUrl()
    {
        var url =
            "https://docs.google.com/spreadsheets/d/1Nq-eGE8isv5rvIYBqqWay4GVoNfBzC5ogxFuQ5pZicQ/edit?gid=0#gid=0";
        var result = await new GoogleDocParser().GetChildrenAsync(url);
        Assert.That(result.IsSuccess, Is.True);
    }
}