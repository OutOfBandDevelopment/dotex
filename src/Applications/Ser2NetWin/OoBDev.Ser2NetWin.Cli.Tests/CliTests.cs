using OoBDev.Ser2NetWin.Cli;
using OoBDev.TestUtilities;

namespace OoBDev.Ser2NetWin.Cli.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class CliTests
{
    [TestMethod]
    public void NoArgumentsMeansRun()
    {
        var options = CliOptions.Parse([]);

        Assert.AreEqual("run", options.Command);
        Assert.IsNull(options.Error);
    }

    [TestMethod]
    public void ParsesCommandConfigAndStart()
    {
        var options = CliOptions.Parse(["install", "--config", @"C:\x\c.json", "--start"]);

        Assert.AreEqual("install", options.Command);
        Assert.AreEqual(@"C:\x\c.json", options.ConfigPath);
        Assert.IsTrue(options.StartAfterInstall);
    }

    [TestMethod]
    public void ReportsUnknownOptionsAndMissingValues()
    {
        Assert.IsNotNull(CliOptions.Parse(["--bogus"]).Error);
        Assert.IsNotNull(CliOptions.Parse(["--config"]).Error);
        Assert.IsNotNull(CliOptions.Parse(["run", "extra"]).Error);
    }

    [TestMethod]
    public void CreateArgumentsQuoteBinPathForSc()
    {
        var args = ServiceInstaller.BuildCreateArguments(@"C:\Program Files\s2n\ser2net-win.exe", @"C:\cfg\my.json");

        Assert.AreEqual(
            "create ser2net-win binPath= \"\\\"C:\\Program Files\\s2n\\ser2net-win.exe\\\" run --config \\\"C:\\cfg\\my.json\\\"\" start= auto DisplayName= \"ser2net-win (serial to TCP bridge)\"",
            args);
    }

    [TestMethod]
    public void CreateArgumentsOmitConfigWhenDefault()
    {
        var args = ServiceInstaller.BuildCreateArguments(@"C:\s2n\ser2net-win.exe", null);

        StringAssert.Contains(args, "binPath= \"\\\"C:\\s2n\\ser2net-win.exe\\\" run\"");
        Assert.DoesNotContain("--config", args);
    }
}
