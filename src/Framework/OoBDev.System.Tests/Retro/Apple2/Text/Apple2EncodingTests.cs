using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Retro.Apple2.Text;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.System.Tests.Retro.Apple2.Text;

[TestClass]
public class Apple2EncodingTests
{
    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void GetBytes_GetString_AllByteValues_RoundTrip()
    {
        // Stage
        var all = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();

        // Test
        var text = Apple2Encoding.Apple2.GetString(all);
        var back = Apple2Encoding.Apple2.GetBytes(text);

        // Assert
        Assert.AreEqual(256, text.Length);
        CollectionAssert.AreEqual(all, back);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetString_HighBitSet_ReturnsPlainAscii()
    {
        // Test
        var text = Apple2Encoding.Apple2.GetString([0xC8, 0xC5, 0xCC, 0xCC, 0xCF]);

        // Assert
        Assert.AreEqual("HELLO", text);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetString_HighBitClear_ReturnsAttributeCharacter()
    {
        // Test
        var text = Apple2Encoding.Apple2.GetString([0x08]);

        // Assert
        Assert.AreEqual((char)(Apple2Encoding.AttributeBase + 0x08), text[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetBytes_PlainAscii_SetsHighBit()
    {
        // Test
        var bytes = Apple2Encoding.Apple2.GetBytes("HELLO");

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0xC8, 0xC5, 0xCC, 0xCC, 0xCF }, bytes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetBytes_UnmappableCharacter_BecomesQuestionMark()
    {
        // Test
        var bytes = Apple2Encoding.Apple2.GetBytes("é");

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0xBF }, bytes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetByteCount_GetCharCount_EqualLength()
    {
        // Assert
        Assert.AreEqual(5, Apple2Encoding.Apple2.GetByteCount("HELLO"));
        Assert.AreEqual(3, Apple2Encoding.Apple2.GetCharCount(new byte[3]));
    }
}
