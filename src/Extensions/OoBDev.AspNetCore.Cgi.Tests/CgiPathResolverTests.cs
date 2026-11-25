using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OoBDev.AspNetCore.Cgi.Tests;

[TestClass]
public class CgiPathResolverTests
{
    private static string Ext => OperatingSystem.IsWindows() ? ".cmd" : ".sh";

    private static CgiPathResolver Resolver() => new(Options.Create(new CgiOptions
    {
        ScriptRoot = Path.Combine(AppContext.BaseDirectory, "Fixtures"),
        AllowBatchFiles = true,
        Handlers = { [Ext] = OperatingSystem.IsWindows() ? "" : "/bin/sh" },
    }));

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("/env", "")]
    [DataRow("/env/extra/path", "/extra/path")]
    [DataRow("env/a/", "/a/")]
    public void Resolve_ScriptWithPathInfo_SplitsNameAndPathInfo(string remaining, string pathInfo)
    {
        // Stage
        var resolver = Resolver();

        // Test
        var script = resolver.Resolve(remaining.Replace("env", "env" + Ext, StringComparison.Ordinal));

        // Assert
        Assert.AreEqual("env" + Ext, script.ScriptName);
        Assert.AreEqual(pathInfo, script.PathInfo);
        Assert.IsTrue(File.Exists(script.ScriptPath));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Resolve_NoScriptNamed_ThrowsNotFound()
    {
        // Test
        var ex = Assert.ThrowsExactly<CgiException>(() => Resolver().Resolve("/"));

        // Assert
        Assert.AreEqual(404, ex.StatusCode);
    }
}
