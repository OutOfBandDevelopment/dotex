using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Retro.Apple2.ApplesoftBASIC;
using OoBDev.TestUtilities;
using System;
using System.Linq;

namespace OoBDev.System.Tests.Retro.Apple2.ApplesoftBASIC;

[TestClass]
public class ApplesoftTokenizerTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Tokenize_Listing_ProducesTokenBytes()
    {
        // Test
        var bytes = ApplesoftTokenizer.Tokenize("10 PRINT \"HI\":GOTO 10");

        // Assert: next-line address = 0x0801 + 4 + 10 + 1 = 0x0810
        byte[] expected =
        [
            0x13, 0x00,
            0x0F, 0x08, 0x0A, 0x00, 0xBA, 0x22, 0x48, 0x49, 0x22, 0x3A, 0xAB, 0x31, 0x30, 0x00,
            0x00, 0x00,
        ];
        expected[0] = (byte)(expected.Length - 2);
        CollectionAssert.AreEqual(expected, bytes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_KeywordInsideName_IsTokenizedLikeTheInterpreter()
    {
        var line = new ApplesoftParser().ParseLine("10 SCORE = 1");

        var elements = line.Statements.Single().Elements;
        Assert.AreEqual("SC", elements[0].Text);
        Assert.AreEqual("OR", elements[1].Text);
        Assert.IsTrue(elements[1].IsKeyword);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_RemAndData_AreLiteral()
    {
        var parser = new ApplesoftParser();

        var rem = parser.ParseLine("5 rem print : goto");
        var data = parser.ParseLine("6 DATA go to,\"a:b\":END");

        Assert.AreEqual(" print : goto", rem.Statements.Single().Elements[1].Text);
        Assert.HasCount(2, data.Statements);
        Assert.AreEqual("go to,\"a:b\"", data.Statements[0].Elements[1].Text.Trim());
        Assert.AreEqual((byte)0x80, data.Statements[1].Elements[0].TokenCode);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_BadLines_Throw()
    {
        var parser = new ApplesoftParser();

        Assert.ThrowsExactly<FormatException>(() => parser.ParseLine("PRINT 1"));
        Assert.ThrowsExactly<FormatException>(() => parser.ParseLine("64000 END"));
        Assert.ThrowsExactly<FormatException>(() => parser.ParseLine("10 REM " + new string('X', 300)));
        Assert.ThrowsExactly<FormatException>(() => parser.ParseListing(new string('1', ApplesoftParser.MaxListingLength + 1)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Tokenize_ThenDetokenize_KeepsTheKeywordsAndLineNumbers()
    {
        var bytes = ApplesoftTokenizer.Tokenize("10 FOR I = 1 TO 3:PRINT I:NEXT I\n20 REM KEEP THIS");

        var lines = new Detokenizer().GetLines(bytes);

        Assert.IsTrue(lines.Any(l => l.StartsWith("10", StringComparison.Ordinal) && l.Contains("FOR", StringComparison.Ordinal) && l.Contains("NEXT", StringComparison.Ordinal)));
        Assert.IsTrue(lines.Any(l => l.StartsWith("20", StringComparison.Ordinal) && l.Contains("KEEP THIS", StringComparison.Ordinal)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Tokenize_DetokenizedOutput_ReproducesTheBytes()
    {
        var first = ApplesoftTokenizer.Tokenize("10 FOR I = 1 TO 3:PRINT I:NEXT I\n30 PRINT \"A B\":GOTO 10");
        var listing = string.Join("\n", new Detokenizer().GetLines(first).Where(l => l.Length > 0 && char.IsAsciiDigit(l[0])));

        var second = ApplesoftTokenizer.Tokenize(listing);

        CollectionAssert.AreEqual(first, second);
    }
}
