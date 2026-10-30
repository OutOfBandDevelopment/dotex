using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;

using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Configuration.Discovery;
namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class TypedInputEchoTests
{
    [TestMethod]
    public void EscapeForDisplay_PlainText_IsUnchanged() => Assert.AreEqual("ID?", TypedInput.EscapeForDisplay("ID?"));

    [TestMethod]
    public void EscapeForDisplay_CarriageReturnAndLineFeed_AreEscaped() =>
        Assert.AreEqual("AT\\r\\n", TypedInput.EscapeForDisplay("AT\r\n"));

    [TestMethod]
    public void EscapeForDisplay_Tab_IsEscaped() => Assert.AreEqual("A\\tB", TypedInput.EscapeForDisplay("A\tB"));

    [TestMethod]
    public void EscapeForDisplay_Nul_IsEscaped() => Assert.AreEqual("A\\0B", TypedInput.EscapeForDisplay("A\0B"));

    [TestMethod]
    public void EscapeForDisplay_OtherControlCharacter_BecomesHexEscape() =>
        Assert.AreEqual("A\\x07B", TypedInput.EscapeForDisplay("A\u0007B"));

    [TestMethod]
    public void EscapeForDisplay_LiteralBackslash_IsDoubledSoItIsNotAmbiguousWithAnEscape() =>
        Assert.AreEqual("A\\\\B", TypedInput.EscapeForDisplay("A\\B"));

    [TestMethod]
    public void FormatForEcho_CrLfLineEnding_AppendsAndEscapesTheTerminator() =>
        Assert.AreEqual("AT\\r\\n", TypedInput.FormatForEcho("AT", LineEnding.CrLf));

    [TestMethod]
    public void FormatForEcho_NoneLineEnding_AddsNothing() =>
        Assert.AreEqual("ID?", TypedInput.FormatForEcho("ID?", LineEnding.None));
}
