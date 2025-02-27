using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;

namespace OoBDev.System.Tests;

[TestClass]
public class ReadOnlySpanExTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(ReadOnlySpanEx), Member = nameof(ReadOnlySpanEx.CopyWithTransform))]
    public void CopyWithTransformTest_byte2byte_7bit()
    {
        byte[] input = [.. Enumerable.Range(0, 255).Select(b => (byte)b)];
        ReadOnlySpan<byte> span = input;
        var result = span.CopyWithTransform(i => (byte)(i & 0x7f));
        foreach (var b in result)
            Assert.IsTrue(b < 0x80);
    }
}
