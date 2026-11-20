using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Spike.PluginSandbox.Tests;

/// <summary>The macOS Seatbelt profile is plain text, so its rules are checked on every OS. Whether macOS enforces them is only known from a run on a Mac.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class SeatbeltProfileTests
{
    [TestMethod]
    public void Profile_Default_DeniesNetworkAndAllWrites()
    {
        var profile = SeatbeltPlugin.Profile("/usr/local/share/dotnet", "/app", [], []);

        StringAssert.Contains(profile, "(deny network*)");
        StringAssert.Contains(profile, "(deny file-write*)");
        Assert.IsFalse(profile.Contains("(allow network", global::System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void Profile_Grants_ReadIsReadOnlyAndWriteIsReadWrite()
    {
        var profile = SeatbeltPlugin.Profile("/d", "/a", ["/data/in"], ["/data/out"]);

        StringAssert.Contains(profile, "(allow file-read* (subpath \"/data/in\"))");
        StringAssert.Contains(profile, "(allow file-read* file-write* (subpath \"/data/out\"))");
        Assert.IsFalse(profile.Contains("file-write* (subpath \"/data/in\")", global::System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void Profile_UserDataAreas_AreDeniedBeforeGrantsAreAllowedBack()
    {
        var profile = SeatbeltPlugin.Profile("/d", "/a", ["/Users/me/in"], []);

        Assert.IsTrue(profile.IndexOf("(deny file-read* (subpath \"/Users\")", global::System.StringComparison.Ordinal) < profile.IndexOf("/Users/me/in", global::System.StringComparison.Ordinal), "Later rules win, so the deny must come first.");
    }

    [TestMethod]
    public void Profile_QuotesInPaths_AreEscaped()
    {
        var profile = SeatbeltPlugin.Profile("/d", "/a", ["/x\"y"], []);

        StringAssert.Contains(profile, "/x\\\"y");
    }

    [TestMethod]
    public void CommandLine_QuotedPart_KeepsSpaces()
    {
        CollectionAssert.AreEqual(new[] { "probe", "file-read", "/a b/c" }, new global::System.Collections.Generic.List<string>(CommandLine.Split("probe file-read \"/a b/c\"")));
    }
}
