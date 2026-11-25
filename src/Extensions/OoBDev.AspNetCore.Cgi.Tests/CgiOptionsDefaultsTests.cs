using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.AspNetCore.Cgi.Tests;

[TestClass]
public class CgiOptionsDefaultsTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void Defaults_BoundRuntimeAndOutput()
    {
        var options = new CgiOptions();

        Assert.AreEqual(TimeSpan.FromSeconds(30), options.Timeout);
        Assert.AreEqual(10L * 1024 * 1024, options.MaxRequestBodyBytes);
        Assert.AreEqual(50L * 1024 * 1024, options.MaxResponseBytes);
        Assert.AreEqual(64 * 1024, options.MaxStderrBytes);
    }
}
