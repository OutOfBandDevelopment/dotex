using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Collections;
using System.Linq;

namespace OoBDev.ZWave.Tests;

[TestClass]
public class ZWaveInterpreterTests
{
    private static ZWaveFrame Data(string hex) => new(ZWaveFrameKind.Data, ZWaveFrameTests.Bytes(hex));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Interpret_VersionReply_NamesTheLibrary()
    {
        var reading = ZWaveInterpreter.Interpret(Data(ZWaveFrameTests.VersionReply));

        StringAssert.Contains(reading.Text, "Z-Wave 2.78, static controller");
        Assert.AreEqual("Z-Wave 2.78, static controller", reading.Values["library"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Interpret_MemoryIdReply_ReadsHomeIdAndNode()
    {
        var reading = ZWaveInterpreter.Interpret(Data(ZWaveFrameTests.MemoryIdReply));

        Assert.AreEqual("016A1C28", reading.Values["homeId"]);
        Assert.AreEqual("1", reading.Values["controllerNode"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Interpret_LoneBytes_AreNamed()
    {
        Assert.AreEqual("ACK", ZWaveInterpreter.Interpret(ZWaveFrame.Ack).Text);
        StringAssert.StartsWith(ZWaveInterpreter.Interpret(new ZWaveFrame(ZWaveFrameKind.Can, [0x18])).Text, "CAN");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Interpret_SwitchReport_DescribesNodeAndState()
    {
        var frame = ZWaveFrame.Create(ZWaveFrame.Request, ZWaveFunctions.ApplicationCommandHandler, [0x00, 0x05, 0x03, 0x25, 0x03, 0xFF]);

        var reading = ZWaveInterpreter.Interpret(frame);

        StringAssert.Contains(reading.Text, "node 5 sent Binary Switch Report on");
        Assert.AreEqual("5", reading.Values["reportNode"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void NodesIn_And_Ranges_ReadTheBitmap()
    {
        var nodes = ZWaveInterpreter.NodesIn([0b0000_0111, 0b0000_0001]);

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 9 }, (ICollection)nodes.ToArray());
        Assert.AreEqual("1-3,9", ZWaveInterpreter.Ranges(nodes));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void DescribeCommand_UnknownClass_FallsBackToHex()
    {
        Assert.AreEqual("class 0xF0 command 0x01 AA", ZWaveInterpreter.DescribeCommand([0xF0, 0x01, 0xAA]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void NodeTable_ProtocolInfoReply_GoesToTheNodeAsked()
    {
        var table = new ZWaveNodeTable();
        table.Expect(ZWaveFunctions.GetNodeProtocolInfo, 7);
        var reply = ZWaveFrame.Create(ZWaveFrame.Response, ZWaveFunctions.GetNodeProtocolInfo, [0xD3, 0x9C, 0x00, 0x04, 0x10, 0x01]);

        var record = table.Apply(reply);

        Assert.IsNotNull(record);
        Assert.AreEqual(7, record.Node);
        StringAssert.Contains(record.Protocol, "Binary Switch");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void NodeTable_MemoryIdAndInitData_FillHomeIdAndNodes()
    {
        var table = new ZWaveNodeTable();
        table.Apply(Data(ZWaveFrameTests.MemoryIdReply));
        table.Apply(ZWaveFrame.Create(ZWaveFrame.Response, ZWaveFunctions.GetInitData, [0x05, 0x08, 0x01, 0b0000_0101, 0x07, 0x00]));

        Assert.AreEqual("016A1C28", table.HomeId);
        CollectionAssert.AreEqual(new[] { 1, 3 }, table.Nodes.Select(n => n.Node).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Requests_NodeManagement_BuildExpectedFunctions()
    {
        Assert.AreEqual(ZWaveFunctions.AddNodeToNetwork, ZWaveRequests.AddNode(1).Function);
        CollectionAssert.AreEqual(new byte[] { 0x05, 0x02 }, ZWaveRequests.StopRemoveNode(2).Payload.ToArray());
        CollectionAssert.AreEqual(new byte[] { 0x09 }, ZWaveRequests.IsFailedNode(9).Payload.ToArray());
    }
}
