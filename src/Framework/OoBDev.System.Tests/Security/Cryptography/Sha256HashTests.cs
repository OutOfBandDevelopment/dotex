using OoBDev.System.Security.Cryptography;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.Tests.Security.Cryptography;

[TestClass]
public class Sha256HashTests
{
    public required TestContext TestContext { get; set; }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Hello World!", "f4OxZX/x/FO5LcGBSKHWXfwtSx+j1ncoSt3SABJtkGk=")]
    [DataRow("hello world", "uU0nuZNNPgilLlLX2n2r+sSE7+N6U4DukIj3rOLvzek=")]
    public void GetHash(string input, string expected)
    {
        var hash = new Sha256Hash();
        var hashed = hash.GetHash(input);
        TestContext.WriteLine($"\"{input}\" => \"{hashed}\"");
        Assert.AreEqual(expected, hashed);
    }
}

