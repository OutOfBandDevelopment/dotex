using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OoBDev.System.IO.Ports;
using OoBDev.TestUtilities;
using SerialPort = System.IO.Ports.SerialPort;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class SerialPortDeviceAdapterTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryOpenMissingPortReturnsFalseTest()
    {
        using var port = new SerialPort("COM9999");
        var adapter = new SerialPortDeviceAdapter(port);

        var opened = adapter.TryOpen(out var stream);

        Assert.IsFalse(opened);
        Assert.IsNull(stream);
    }

    [TestMethod, TestCategory(TestCategories.Hardware)]
    public void TryOpenPortHeldByAnotherHandleReturnsFalseTest()
    {
        var names = SerialPort.GetPortNames();
        if (names.Length == 0) Assert.Inconclusive("No serial port detected.");

        using var holder = new SerialPort(names[0]);
        try { holder.Open(); }
        catch (Exception) { Assert.Inconclusive($"{names[0]} is already in use or cannot be opened."); }

        using var second = new SerialPort(names[0]);
        var opened = new SerialPortDeviceAdapter(second).TryOpen(out var stream);

        Assert.IsFalse(opened);
        Assert.IsNull(stream);
    }
}
