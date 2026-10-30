using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Wpf;
using OoBDev.System.IO.Bluetooth;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The scan commands of the connection editor, driven with fake scanners.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ConnectionDeviceScansTests
{
    private string _dir = string.Empty;

    [TestInitialize]
    public void Setup() => _dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}")).FullName;

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_dir, recursive: true);

    private ConnectionEditorViewModel CreateEditor() =>
        new(new ConnectionProfileStore(_dir), new CliOptions { Transport = "tcp" });

    [TestMethod]
    public void ScanLxi_Executed_FillsTheDetectedNetworkDevices()
    {
        // Stage
        using var editor = CreateEditor();
        var scans = new ConnectionDeviceScans(editor, lxi: () => [new LxiDeviceOption("Scope (192.168.0.110)", "192.168.0.110", 23)]);

        // Test
        scans.ScanLxi.Execute(null);

        // Assert
        Assert.AreEqual(1, editor.LxiDeviceOptions.Count);
        Assert.AreEqual("192.168.0.110", editor.LxiDeviceOptions[0].Host);
    }

    [TestMethod]
    public void ScanBle_Executed_FillsTheDetectedBleDevices()
    {
        // Stage
        using var editor = CreateEditor();
        var scans = new ConnectionDeviceScans(editor, ble: () => [new BleDeviceOption("Meter", "AA:BB", "Meter")]);

        // Test
        scans.ScanBle.Execute(null);

        // Assert
        Assert.AreEqual(1, editor.BleDeviceOptions.Count);
        Assert.AreEqual("AA:BB", editor.BleDeviceOptions[0].DeviceId);
    }

    [TestMethod]
    public void ExploreBle_Executed_ReadsTheCharacteristicsOfTheEditorDevice()
    {
        // Stage
        using var editor = CreateEditor();
        editor.BleDeviceId = "AA:BB";
        string? asked = null;
        var scans = new ConnectionDeviceScans(editor, characteristics: id =>
        {
            asked = id;
            return Array.Empty<BleGattServiceDescriptor>();
        });

        // Test
        scans.ExploreBle.Execute(null);

        // Assert
        Assert.AreEqual("AA:BB", asked);
        Assert.AreEqual(0, editor.BleCharacteristicOptions.Count);
    }

    [TestMethod]
    public void Constructor_NullEditor_Throws()
    {
        // Stage / Test / Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new ConnectionDeviceScans(null!));
    }
}
