using System.IO;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.Ser2NetWin.Cli.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class UsbBridgeRemovalTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void RejectRemovedUsbBridges_ConfigWithUsbBridges_FailsAndPointsToTheRemoteDevTermHost()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, "ser2net.json");
        File.WriteAllText(path, """{ "UsbBridges": [ { "Kind": "UsbTmc", "TcpPort": 4100 } ] }""");

        // Test
        var error = Assert.ThrowsExactly<InvalidOperationException>(() => HostRunner.RejectRemovedUsbBridges(path));
        TestContext.WriteLine(error.Message);

        // Assert
        StringAssert.Contains(error.Message, "remote DevTerm host");
    }

    [TestMethod]
    public void RejectRemovedUsbBridges_ConfigWithoutTheSectionOrFile_Passes()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, "ser2net.json");
        File.WriteAllText(path, """{ "Bridges": [] }""");

        // Test
        HostRunner.RejectRemovedUsbBridges(path);
        HostRunner.RejectRemovedUsbBridges(Path.Combine(folder.DirectoryPath, "missing.json"));

        // Assert
        TestContext.WriteLine("no exception for a config without UsbBridges or a missing file");
    }
}
