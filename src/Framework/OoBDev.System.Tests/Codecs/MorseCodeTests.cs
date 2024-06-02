using OoBDev.System.Codecs;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.Codecs;

[TestClass]
public class MorseCodeTests
{
    public required TestContext TestContext { get; set; }

    [DataTestMethod]
    [DataRow("Hello, World!", ".... . .-.. .-.. ---  .-- --- .-. .-.. -..")]
    [DataRow("hello world", ".... . .-.. .-.. ---  .-- --- .-. .-.. -..")]
    [DataRow("abcdefghijklmnopqrstuvwxyz1234567890", ".- -... -.-. -.. . ..-. --. .... .. .--- -.- .-.. -- -. --- .--. --.- .-. ... - ..- ...- .-- -..- -.-- --.. .---- ..--- ...-- ....- ..... -.... --... ---.. ----. -----")]
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EncodeTest(string message, string expected)
    {
        var result = new MorseCode().Encode(message);
        TestContext.WriteLine($"{message} -> {result}");
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [DataRow(".... . .-.. .-.. ---  .-- --- .-. .-.. -..", "HELLO WORLD")]
    [DataRow(".- -... -.-. -.. . ..-. --. .... .. .--- -.- .-.. -- -. --- .--. --.- .-. ... - ..- ...- .-- -..- -.-- --..  .---- ..--- ...-- ....- ..... -.... --... ---.. ----. -----", "ABCDEFGHIJKLMNOPQRSTUVWXYZ 1234567890")]
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void DecodeTest(string message, string expected)
    {
        var result = new MorseCode().Decode(message);
        TestContext.WriteLine($"{message} -> {result}");
        Assert.AreEqual(expected, result);
    }
}
