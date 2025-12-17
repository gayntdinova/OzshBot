using OzshBot.Infrastructure;

namespace Tests.InfrastructureTests.ParserTests;

[TestFixture]
public class PhoneParserTests
{

    [TestCase("some string")]
    [TestCase("")]
    [TestCase("+21111111111")]
    public void NormalizePhone_FailsOnInvalidInput(string input)
    {
        Assert.That(() => PhoneParser.NormalizePhone(input), Throws.ArgumentException);
    }

    [TestCase("+71111111111", "+71111111111")]
    [TestCase("81111111111", "+71111111111")]
    [TestCase("8(111)111-11-11", "+71111111111")]
    public void NormalizePhone_Succeeds(string input, string expected)
    {
        Assert.That(PhoneParser.NormalizePhone(input), Is.EqualTo(expected));
    }

    public void NormalizePhone_Fails_WithNullInput()
    {
        Assert.That(() => PhoneParser.NormalizePhone(null), Throws.ArgumentNullException);
    }

    [TestCaseSource(nameof(TestCases))]
    public void ExtractAllPhones_Succeeds(string input, List<string> expected)
    {
        Assert.That(PhoneParser.ExtractAllPhones(input), Is.EqualTo(expected));
    }
    
    static IEnumerable<TestCaseData> TestCases =>
        new[]
        {
            new TestCaseData(
                "",
                new List<string> ()
            ),
            new TestCaseData(
                "biba 8(111)111-11-11 aboba +7(222)111-11-11 boba",
                new List<string>(){"+71111111111", "+72221111111"}
            )
        };
}