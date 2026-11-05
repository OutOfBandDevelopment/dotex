using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using OoBDev.System.IO;
using OoBDev.System.IO.Ports;
using OoBDev.TestUtilities;
using SerialPort = global::System.IO.Ports.SerialPort;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class SerialPortDeviceAdapterTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task OpenAsyncMissingPortFaultsTest()
    {
        using var port = new SerialPort("COM9999");
        var adapter = new SerialPortDeviceAdapter(port);

        await Assert.ThrowsExactlyAsync<IOException>(() => adapter.OpenAsync());

        Assert.AreEqual(ConnectionState.Faulted, adapter.State);
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public async Task OpenAsyncPortHeldByAnotherHandleFaultsTest()
    {
        var names = SerialPort.GetPortNames();
        if (names.Length == 0) Assert.Inconclusive("No serial port detected.");

        using var holder = new SerialPort(names[0]);
        try { holder.Open(); }
        catch (Exception) { Assert.Inconclusive($"{names[0]} is already in use or cannot be opened."); }

        using var second = new SerialPort(names[0]);
        var adapter = new SerialPortDeviceAdapter(second);

        await Assert.ThrowsExactlyAsync<IOException>(() => adapter.OpenAsync());

        Assert.AreEqual(ConnectionState.Faulted, adapter.State);
    }
}
