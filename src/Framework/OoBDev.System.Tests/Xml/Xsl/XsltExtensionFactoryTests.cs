using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Xml.Xsl;
using OoBDev.TestUtilities;
using System;
using System.Diagnostics;
using System.Reflection;

namespace OoBDev.System.Tests.Xml.Xsl;

[TestClass]
public class XsltExtensionFactoryTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("do-work", new string[] { "Hi!" }, "Hi!")]
    [DataRow("big-work", new string[] { "Hi!", "2", "3", "4", "5", "6" }, "Hi!_2_3_4_5_6")]
    [DataRow("more-work", new string[] { "Hi!" }, null)]
    [DataRow("other-work", new string[] { }, null)]
    [DataRow("and-work", new string[] { }, "noice")]
    public void BuildXsltExtensionTest(string method, string[] inputs, string? expected)
    {
        var factory = new XsltExtensionFactory();

        var toWrap = new FakeClass();

        var wrapped = factory.BuildXsltExtension(toWrap) ?? throw new NotSupportedException();
        var wrappedType = wrapped.GetType();
        var mi = wrappedType.GetMethod(method, BindingFlags.Public | BindingFlags.Instance);
        var ret = mi?.Invoke(wrapped, inputs);
        TestContext.WriteLine($"{method}: {ret}");

        if (expected != null)
        {
            Assert.AreEqual(expected, ret as string);
        } else
        {
            Assert.IsNull(ret);
        }
    }

    public class FakeClass
    {
        [XsltFunction("big-work")]
        public string DoWork3(string x1, string x2, string x3, string x4, string x5, string x6) => string.Join("_", x1, x2, x3, x4, x5, x6);

        [XsltFunction("do-work")]
        public string DoWork(string input) => input;

        [XsltFunction("more-work")]
        public void MoreWork(string input) => Debug.WriteLine(input);

        [XsltFunction("other-work")]
        public void OtherWork() => Debug.WriteLine("hello!");

        [XsltFunction("and-work")]
        public string AndWork() => "noice";
    }
}
