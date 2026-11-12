using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Retro.Apple2.ApplesoftBASIC;
using OoBDev.TestUtilities;
using System;
using System.Linq;

namespace OoBDev.System.Tests.Retro.Apple2.ApplesoftBASIC;

[TestClass]
public class DetokenizerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(Detokenizer), Member = nameof(Detokenizer.GetLines))]
    public void GetLinesTest()
    {
        //Stage
        var example = Convert.FromBase64String(@"owEJCAoAiTqXAB4IFABEJNDnKDQpOrIgQ1RSTC1EADkIHgCiMjpBJNAiQVBQTEUgSUkiOrAxMDAw
AGoIKACiNDpBJNAiRE9TIFZFUlNJT04gMy4zICBTWVNURU0gTUFTVEVSIjqwMTAwMACMCDIAojc6
QSTQIkpBTlVBUlkgMSwgMTk4MyI6sDEwMDAAqAg8ALpEJDsiQkxPQUQgTE9BREVSLk9CSjAiAM8I
RgCMNDA5NjqyIEZBU1QgTE9BRCBJTiBJTlRFR0VSIEJBU0lDABAJUACiMTA6jMk5NTg6QSTQIkNP
UFlSSUdIVCBBUFBMRSBDT01QVVRFUixJTkMuIDE5ODAsMTk4MiI6sDEwMDAATwlaAEPQ4ijJMTEw
MSk6rUPQNsS6Op46QSTQIkJFIFNVUkUgQ0FQUyBMT0NLIElTIERPV04iOrAxMDAwOp0AXglkALrn
KDQpOyJGUCIAdQnoA7IgQ0VOVEVSIFNUUklORyBBJACVCfIDQtDTKDIwySjjKEEkKcsyKSk6rULQ
0TDEQtAxAKIJ/AOWQjq6QSQ6sQAAAIcAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");

        var bytes = example.Count(b => b != 0);

        //Mock

        //Test
        var detokenizer = new Detokenizer();
        var lines = detokenizer.GetLines(example);

        //Output
        var result = string.Join("\r\n", lines);
        TestContext.WriteLine(result);

        //Assert
        var line9 = lines.ElementAt(9);
        var expected = "90 C= PEEK ( - 1101):IF C= 6 THEN PRINT :INVERSE :A$= \"BE SURE CAPS LOCK IS DOWN\":GOSUB 1000:NORMAL ";
        Assert.AreEqual(expected, line9);

        //Verify
    }
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetLines_Empty_ReturnsOnlyTheSizeLine()
    {
        var lines = new Detokenizer().GetLines(ReadOnlyMemory<byte>.Empty);

        Assert.AreEqual("$$$ FILE SIZE :0", lines.Single());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetLines_TruncatedLine_EndsTheLineWithoutThrowing()
    {
        // size 5, next address 0x0801, line number 10, then "PR" with no terminator
        byte[] input = [5, 0, 0x01, 0x08, 10, 0, (byte)'P', (byte)'R'];

        var lines = new Detokenizer().GetLines(input);

        Assert.AreEqual("10 PR", lines[1]);
    }
}
