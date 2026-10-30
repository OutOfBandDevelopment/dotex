using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Buffers;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Velleman.K8055.Panels.Tests;

[TestClass]
public sealed class K8055PanelTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("hid", 0x10CF, 0x5500, true)]
    [DataRow("HID", 0x10CF, 0x5503, true)]
    [DataRow("hid", 0x10CF, 0x5504, false)]
    [DataRow("tcp", 0x10CF, 0x5500, false)]
    [DataRow("hid", 0x04D8, 0xF848, false)]
    public void IsAvailable_K8055Boards_OnlyForAVellemanHidBoardAddress(string transport, int vendorId, int productId, bool expected) =>
        Assert.AreEqual(expected, new K8055Panel().IsAvailable(transport, vendorId, productId));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Identity_IsTheK8055PanelWithItsDecoder()
    {
        var panel = new K8055Panel();

        Assert.AreEqual("k8055", panel.Id);
        Assert.AreEqual(panel.PresenterName, panel.CreateDecoder().Name);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void BuildDefinition_CommandControls_AreAllHandledBySurface()
    {
        var definition = new K8055Panel().BuildDefinition();
        var surface = new K8055ControlSurface((_, _) => Task.CompletedTask);

        var commands = definition.Sections.SelectMany(s => s.Controls).Select(c => c.Id).Where(id => id.StartsWith("digitalOut") || id.StartsWith("analogOut") || id.StartsWith("resetCounter")).ToList();

        Assert.IsNotEmpty(commands);
        foreach (var id in commands)
        {
            Assert.IsNotNull(surface.PreviewCommand(id, "1"), id);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task CreateSurface_Invoke_SendsThroughTheDelegate()
    {
        byte[]? sent = null;
        var surface = new K8055Panel().CreateSurface((data, _) =>
        {
            sent = data.ToArray();
            return Task.CompletedTask;
        });

        await surface.InvokeAsync("resetCounter2", null, TestContext.CancellationToken);

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x04, 0, 0, 0, 0, 0, 0, 0 }, sent);
    }

    public required TestContext TestContext { get; set; }
}
