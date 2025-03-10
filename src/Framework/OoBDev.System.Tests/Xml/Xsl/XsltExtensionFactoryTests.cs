﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Xml.Xsl;
using OoBDev.TestUtilities;
using System.Diagnostics;
using System.Reflection;

namespace OoBDev.System.Tests.Xml.Xsl;

[TestClass]
public class XsltExtensionFactoryTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public void BuildXsltExtensionTest()
    {
        var factory = new XsltExtensionFactory();

        var toWrap = new FakeClass();

        var wrapped = factory.BuildXsltExtension(toWrap);
        var wrappedType = wrapped.GetType();

        {
            var mi = wrappedType.GetMethod("do-work", BindingFlags.Public | BindingFlags.Instance);
            var ret = mi?.Invoke(wrapped, ["Hi!"]);
            TestContext.WriteLine($"{"do-work"}: {ret}");
        }
        {
            var mi = wrappedType.GetMethod("big-work", BindingFlags.Public | BindingFlags.Instance);
            var ret = mi?.Invoke(wrapped, ["Hi!", "2", "3", "4", "5", "6"]);
            TestContext.WriteLine($"{"big-work"}: {ret}");
        }
        {
            var mi = wrappedType.GetMethod("more-work", BindingFlags.Public | BindingFlags.Instance);
            var ret = mi?.Invoke(wrapped, ["Hi!"]);
            TestContext.WriteLine($"{"more-work"}: {ret}");
        }
        {
            var mi = wrappedType.GetMethod("other-work", BindingFlags.Public | BindingFlags.Instance);
            var ret = mi?.Invoke(wrapped, []);
            TestContext.WriteLine($"{"other-work"}: {ret}");
        }
        {
            var mi = wrappedType.GetMethod("and-work", BindingFlags.Public | BindingFlags.Instance);
            var ret = mi?.Invoke(wrapped, []);
            TestContext.WriteLine($"{"and-work"}: {ret}");
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
