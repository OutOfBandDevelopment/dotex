using Microsoft.VisualStudio.TestTools.UnitTesting;
using NuGet.Protocol;
using System;
using System.IO;

namespace OoBDev.PackMan.Cli.Tests;

[TestClass]
public class PackManOptionsTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_NoArguments_UsesDefaults()
    {
        var o = PackManOptions.Parse([]);

        Assert.AreEqual(Path.Combine(Environment.CurrentDirectory, "Directory.Packages.props"), o.SourceFile);
        Assert.AreEqual(PackManOptions.DefaultNugetUrl, o.NugetUrl);
        Assert.AreEqual(FeedType.HttpV3, o.FeedType);
        Assert.IsFalse(o.NoSave);
        Assert.IsFalse(o.NoNuget);
        Assert.IsFalse(o.UpdateProjects);
        Assert.IsNull(o.OutputFile);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_ShortAndLongSwitches_FillEveryOption()
    {
        var o = PackManOptions.Parse([
            "-s", "in.props", "-o", "out.props", "-n", "--no-nuget", "-t", "A,1.0.0;B", "--nuget-url", "http://x/index.json",
            "--nuget-feed-type", "HttpV2", "--get-properties", "--follow-imports", "-g", "A", "--update-projects",
            "--projects", "**/*.csproj;!**/obj/**", "--projects-path", "src"]);

        Assert.AreEqual("in.props", o.SourceFile);
        Assert.AreEqual("out.props", o.OutputFile);
        Assert.IsTrue(o.NoSave);
        Assert.IsTrue(o.NoNuget);
        Assert.AreEqual("A,1.0.0;B", o.TargetVersions);
        Assert.AreEqual("http://x/index.json", o.NugetUrl);
        Assert.AreEqual(FeedType.HttpV2, o.FeedType);
        Assert.IsTrue(o.GetProperties);
        Assert.IsTrue(o.FollowImports);
        Assert.AreEqual("A", o.GetProperty);
        Assert.IsTrue(o.UpdateProjects);
        Assert.AreEqual("**/*.csproj;!**/obj/**", o.Projects);
        Assert.AreEqual("src", o.ProjectsPath);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_FlagBeforeAnotherSwitch_IsTrue()
    {
        var o = PackManOptions.Parse(["--no-nuget", "-s", "a.props", "-v"]);

        Assert.IsTrue(o.NoNuget);
        Assert.IsTrue(o.GetVersion);
        Assert.AreEqual("a.props", o.SourceFile);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_UnknownFeedType_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => PackManOptions.Parse(["--nuget-feed-type", "nope"]));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_NullArgs_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => PackManOptions.Parse(null!));
    }
}
