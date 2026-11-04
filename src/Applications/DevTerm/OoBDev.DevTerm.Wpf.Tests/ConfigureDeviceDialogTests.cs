using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO.Devices;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The Configure device window over a device configuration editor.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ConfigureDeviceDialogTests
{
    private sealed class FakeEditor : IDeviceConfigEditor
    {
        public Dictionary<string, string> Device { get; } = new() { ["ip"] = "192.168.0.201", ["baud"] = "9600", ["mode"] = "tcp-server", ["dhcp"] = "false" };

        public string Id => "fake";

        public string Title => "Fake bridge";

        public UiDefinition BuildDefinition() => new()
        {
            Name = "Fake bridge",
            Sections =
            [
                new UiSection
                {
                    Label = "Network",
                    Controls =
                    [
                        new TextFieldControl { Id = "ip", Label = "IP address" },
                        new TextFieldControl { Id = "baud", Label = "Baud" },
                        new ChoiceControl { Id = "mode", Label = "Work mode", Options = ["tcp-server", "tcp-client", "udp"] },
                        new ToggleControl { Id = "dhcp", Label = "DHCP" },
                    ],
                },
            ],
        };

        public Task<IReadOnlyDictionary<string, string>> ReadAsync(DeviceConfigTarget target, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>(Device));

        public IReadOnlyList<DeviceConfigIssue> Validate(IReadOnlyDictionary<string, string> values) =>
            values.GetValueOrDefault("baud") == "0" ? [new DeviceConfigIssue("baud", "must be positive")] : [];

        public bool WillDropConnection(DeviceConfigTarget target, IReadOnlyDictionary<string, string> current, IReadOnlyDictionary<string, string> values) =>
            current["ip"] != values["ip"];

        public Task<DeviceConfigWriteResult> WriteAsync(DeviceConfigTarget target, IReadOnlyDictionary<string, string> values, CancellationToken cancellationToken)
        {
            foreach (var (key, value) in values)
            {
                Device[key] = value;
            }

            return Task.FromResult(new DeviceConfigWriteResult(new Dictionary<string, string>(Device), RebootRequired: true));
        }
    }

    private sealed class FakeConfigureDeviceDialog(Action<ConfigureDeviceDialogViewModel> interact) : IConfigureDeviceDialog
    {
        public void Show(ConfigureDeviceDialogViewModel device) => interact(device);
    }

    [TestMethod]
    public async Task ReadAsync_OneEditor_BuildsATypedFormFromTheDevice()
    {
        // Stage
        var device = new ConfigureDeviceDialogViewModel([new FakeEditor()], "10.0.0.5", "8899");

        // Test
        await device.ReadAsync();

        // Assert
        Assert.AreEqual(4, device.Fields.Count);
        Assert.IsInstanceOfType<ConfigTextFieldViewModel>(device.Fields[0]);
        Assert.IsInstanceOfType<ConfigChoiceFieldViewModel>(device.Fields[2]);
        Assert.IsInstanceOfType<ConfigToggleFieldViewModel>(device.Fields[3]);
        Assert.AreEqual("192.168.0.201", ((ConfigTextFieldViewModel)device.Fields[0]).Text);
        Assert.IsFalse(device.Write.CanExecute(null));
    }

    [TestMethod]
    public async Task Text_EditedIp_ListsTheChangeAndWarnsAboutTheConnection()
    {
        // Stage
        var device = new ConfigureDeviceDialogViewModel([new FakeEditor()], "10.0.0.5", "8899");
        await device.ReadAsync();

        // Test
        ((ConfigTextFieldViewModel)device.Fields[0]).Text = "192.168.0.202";

        // Assert
        StringAssert.Contains(device.Summary, "change: ip: 192.168.0.201 to 192.168.0.202");
        StringAssert.Contains(device.Summary, "connection will drop");
        Assert.IsTrue(device.Write.CanExecute(null));
    }

    [TestMethod]
    public async Task Text_InvalidBaud_ReportsTheProblemAndBlocksTheWrite()
    {
        // Stage
        var device = new ConfigureDeviceDialogViewModel([new FakeEditor()], "10.0.0.5", "8899");
        await device.ReadAsync();

        // Test
        ((ConfigTextFieldViewModel)device.Fields[1]).Text = "0";

        // Assert
        StringAssert.Contains(device.Summary, "problem: baud: must be positive");
        Assert.IsFalse(device.Write.CanExecute(null));
    }

    [TestMethod]
    public async Task WriteAsync_EditedChoiceAndToggle_WritesAndConfirmsTheReadBack()
    {
        // Stage
        var editor = new FakeEditor();
        var device = new ConfigureDeviceDialogViewModel([editor], "10.0.0.5", "8899");
        await device.ReadAsync();
        ((ConfigChoiceFieldViewModel)device.Fields[2]).Selected = "udp";
        ((ConfigToggleFieldViewModel)device.Fields[3]).IsOn = true;

        // Test
        await device.WriteAsync();

        // Assert
        Assert.AreEqual("udp", editor.Device["mode"]);
        Assert.AreEqual("true", editor.Device["dhcp"]);
        StringAssert.Contains(device.Status, "the device confirmed every value");
        StringAssert.Contains(device.Status, "Restart the device");
    }

    [TestMethod]
    public async Task ReadAsync_NoEditorChosen_AsksForOne()
    {
        // Stage
        var device = new ConfigureDeviceDialogViewModel([new FakeEditor(), new FakeEditor()]);

        // Test
        await device.ReadAsync();

        // Assert
        Assert.AreEqual("Choose an editor first.", device.Status);
        Assert.IsFalse(device.Read.CanExecute(null));
    }

    [TestMethod]
    public void ShowConfigureDevice_NoEditorsRegistered_IsDisabled()
    {
        // Stage
        var shell = new ShellViewModel([], configureDeviceDialog: new FakeConfigureDeviceDialog(_ => { }));

        // Test
        var enabled = shell.ShowConfigureDevice.CanExecute(null);

        // Assert
        Assert.IsFalse(enabled);
    }

    [TestMethod]
    public void ShowConfigureDevice_EditorRegistered_ShowsTheWindow()
    {
        // Stage
        IReadOnlyList<IDeviceConfigEditor>? shown = null;
        var shell = new ShellViewModel([], configureDeviceDialog: new FakeConfigureDeviceDialog(d => shown = d.Editors), configEditors: [new FakeEditor()]);

        // Test
        shell.ShowConfigureDevice.Execute(null);

        // Assert
        Assert.AreEqual("fake", shown!.Single().Id);
    }
}
