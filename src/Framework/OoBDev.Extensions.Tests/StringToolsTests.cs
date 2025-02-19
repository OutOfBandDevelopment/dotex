using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace OoBDev.Extensions.Tests;

[TestClass]
public class StringToolsTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("abcdefghijklmnop", @"abcdefghij
klmnop")]
    [DataRow("abcde fghijklmnop", @"abcde
fghijklmno
p")]
    [DataRow("            ", @"
")]
    [DataRow("a            ", @"a
")]
    [DataRow("a            b", @"a
   b")]
    [DataRow("", "")]
    public void Test(string input, string expected)
    {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            var result = input.SplitBy(length: 10).WriteAsLines();
            Assert.AreEqual(expected, result);
        }
        else
        {
            if (string.IsNullOrEmpty(input))
            {
                var result = input.SplitBy(length: 10).Any();
                Assert.IsFalse(result);
            }
            else
            {
                var result = input.SplitBy(length: 10).Any(i => i.Length <= 10);
                Assert.IsTrue(result);
            }
        }
    }
}
