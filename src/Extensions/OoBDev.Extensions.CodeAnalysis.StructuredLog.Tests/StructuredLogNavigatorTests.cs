using System;
using System.Diagnostics;
using System.IO;
using System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Extensions.CodeAnalysis.StructuredLog.Tests;

/// <summary>Navigates a binary log written by a real <c>dotnet msbuild</c> run of a one-target project.</summary>
[TestClass]
public class StructuredLogNavigatorTests
{
    private const string ProjectXml = """
        <Project>
          <Target Name="Sample">
            <Message Importance="high" Text="hello log" />
            <Warning Text="careful" Code="SAMPLE1" />
          </Target>
        </Project>
        """;

    private string _directory = null!;
    private string _binlog = null!;

    [TestInitialize]
    public void CreateLog()
    {
        _directory = Path.Combine(Path.GetTempPath(), $"Test_{(global::System.Guid.NewGuid()):N}");
        Directory.CreateDirectory(_directory);
        var project = Path.Combine(_directory, "sample.proj");
        _binlog = Path.Combine(_directory, "sample.binlog");
        File.WriteAllText(project, ProjectXml);

        var start = new ProcessStartInfo("dotnet", ["msbuild", project, "-t:Sample", $"-bl:{_binlog}", "-nologo", "-v:q"])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        using var process = Process.Start(start)!;
        process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.IsTrue(File.Exists(_binlog), "msbuild did not write a binary log");
    }

    [TestCleanup]
    public void DeleteLog() => Directory.Delete(_directory, recursive: true);

    private static int Count(XPathNavigator navigator, string xpath) => navigator.Select(xpath).Count;

    [TestMethod]
    [TestCategory("Simulate")]
    public void ToNavigable_File_ExposesBuildProjectAndTarget()
    {
        // Stage
        var navigator = new StructuredLogNavigator().ToNavigable(_binlog)!.CreateNavigator()!;

        // Test
        var targets = Count(navigator, "/Build//Target[@Name='Sample']");

        // Assert
        Assert.AreEqual(1, targets);
        Assert.AreEqual(1, Count(navigator, "/Build[@Succeeded='True']"));
        Assert.IsTrue(Count(navigator, "/Build/Project") >= 1);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public void ToNavigable_File_ReportsMessagesAndWarnings()
    {
        var navigator = new StructuredLogNavigator().ToNavigable(_binlog)!.CreateNavigator()!;

        Assert.IsTrue(Count(navigator, "//Message[contains(@Text,'hello log')]") >= 1);
        Assert.IsTrue(Count(navigator, "//Warning[@Code='SAMPLE1']") >= 1);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public void ToNavigable_Stream_ReadsSameLog()
    {
        using var stream = File.OpenRead(_binlog);

        var navigator = new StructuredLogNavigator().ToNavigable(stream)!.CreateNavigator()!;

        Assert.AreEqual(1, Count(navigator, "/Build//Target[@Name='Sample']"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void AsNode_NullBuild_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => StructuredLogNavigatorFactory.AsNode(null!));
    }
}
