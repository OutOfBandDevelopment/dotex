using OoBDev.System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.DevTerm;

using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Configuration.Discovery;
namespace OoBDev.DevTerm.Configuration.Tests;

/// <summary>
/// The shared, front-end-agnostic pieces behind the main windows' connection-state UI: which
/// Device panels are available, the title's disconnected suffix, the status-bar text, and the SCPI
/// auto-detect timeout option.
/// </summary>
[TestCategory("Unit")]
[TestClass]
public sealed class MainWindowStateTests
{
    private static CliOptions Hid(int vendorId, int productId) => new() { Transport = "hid", VendorId = vendorId, ProductId = productId };

    [TestMethod]
    public void Manifest_ForAnyConnection()
    {
        Assert.IsTrue(DevicePanels.IsAvailable(DevicePanel.Manifest, new CliOptions { Transport = "loopback" }, connected: true));
        Assert.IsTrue(DevicePanels.IsAvailable(DevicePanel.Manifest, Hid(0x10CF, 0x5500), connected: true));
    }

    [TestMethod]
    public void NoPanelWhileDisconnected()
    {
        Assert.IsFalse(DevicePanels.IsAvailable(DevicePanel.Manifest, Hid(0x10CF, 0x5500), connected: false));
        Assert.IsFalse(DevicePanels.IsAvailable(DevicePanel.Manifest, new CliOptions { Transport = "loopback" }, connected: false));
    }

    [TestMethod]
    public void UnavailableReason_ExplainsAPanelWhoseFeedingPresenterIsNotSelected()
    {
        var panel = new Mock<IDevicePanelContribution>();
        panel.SetupGet(p => p.PresenterName).Returns("k8055");
        panel.Setup(p => p.IsAvailable(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(true);

        var hex = new CliOptions { Transport = "hid", Presenter = ["hex"] };
        var selected = new CliOptions { Transport = "hid", Presenter = ["hex", "K8055"] };

        Assert.AreEqual("Connect first.", DevicePanels.UnavailableReason(panel.Object, selected, connected: false));
        Assert.AreEqual("Select the 'k8055' presenter to use this panel.", DevicePanels.UnavailableReason(panel.Object, hex, connected: true));
        Assert.IsNull(DevicePanels.UnavailableReason(panel.Object, selected, connected: true));

        panel.Setup(p => p.IsAvailable(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>())).Returns(false);
        Assert.AreEqual("This panel does not suit the current connection.", DevicePanels.UnavailableReason(panel.Object, selected, connected: true));
    }

    [TestMethod]
    public void WindowTitle_SaysDisconnectedWhenItIs()
    {
        var options = new CliOptions { Transport = "tcp", Host = "192.168.0.110", Port = "23", Presenter = ["ascii"] };
        var store = new ConnectionProfileStore(Path.Combine(Path.GetTempPath(), "devterm-title-tests", Path.GetRandomFileName()));

        Assert.DoesNotContain("disconnected", ConnectionDescription.WindowTitle(options, "ascii", store, connected: true));
        Assert.EndsWith(" — disconnected", ConnectionDescription.WindowTitle(options, "ascii", store, connected: false));
    }

    [TestMethod]
    [DataRow(ConnectionState.Open, "Connected — tcp://192.168.0.110:23")]
    [DataRow(ConnectionState.Opening, "Connecting — tcp://192.168.0.110:23")]
    [DataRow(ConnectionState.Closed, "Disconnected — tcp://192.168.0.110:23")]
    [DataRow(ConnectionState.Faulted, "Disconnected — tcp://192.168.0.110:23")]
    public void StatusText_NamesTheStateAndTheConnection(ConnectionState state, string expected) =>
        Assert.AreEqual(expected, ConnectionDescription.StatusText(new CliOptions { Transport = "tcp", Host = "192.168.0.110", Port = "23" }, state));

    [TestMethod]
    [DataRow(99, false)]
    [DataRow(100, true)]
    [DataRow(60000, true)]
    [DataRow(60001, false)]
    public void ScpiAutoDetectTimeout_IsValidated(int timeoutMs, bool valid)
    {
        var options = new CliOptions { Transport = "loopback", ScpiAutoDetectTimeoutMs = timeoutMs };

        Assert.AreEqual(valid, new CliOptionsValidator().Validate(null, options).Succeeded);
    }

    [TestMethod]
    public void ScpiAutoDetectTimeout_SavedInAProfileOnlyWhenNotTheDefault()
    {
        Assert.DoesNotContain(nameof(CliOptions.ScpiAutoDetectTimeoutMs), DevTermConfiguration.ToProfileJson(new CliOptions { Transport = "loopback" }));
        Assert.Contains("\"ScpiAutoDetectTimeoutMs\": 8000", DevTermConfiguration.ToProfileJson(new CliOptions { Transport = "loopback", ScpiAutoDetectTimeoutMs = 8000 }));
    }
}
