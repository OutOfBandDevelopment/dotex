using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DevTerm.Tests;

/// <summary>Verifies the shared preview formatting every <see cref="ICommandPreview"/> implementation uses.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class CommandPreviewFormatTests
{
    [TestMethod]
    public void EscapeControlCharacters_EscapesTerminatorsAndOtherControlCharactersVisibly()
    {
        Assert.AreEqual("*IDN?\\r\\n", CommandPreviewFormat.EscapeControlCharacters("*IDN?\r\n"));
        Assert.AreEqual("A\\tB\\x1BC\\\\D", CommandPreviewFormat.EscapeControlCharacters("A\tB\u001bC\\D"));
        Assert.AreEqual("MEAS:VOLT:DC? DEF", CommandPreviewFormat.EscapeControlCharacters("MEAS:VOLT:DC? DEF"));
    }

    [TestMethod]
    public void ToHex_IsSpaceSeparatedUpperCaseBytes()
    {
        Assert.AreEqual("00 05 FF 0A", CommandPreviewFormat.ToHex([0x00, 0x05, 0xFF, 0x0A]));
        Assert.AreEqual(string.Empty, CommandPreviewFormat.ToHex([]));
    }
}
