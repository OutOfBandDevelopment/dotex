using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>Device panels contributed by plugins, opened for the selected session.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class DevicePanelTests
{
    private sealed class FakeSurface : IControlSurface, ICommandPreview
    {
        public List<(string Id, string? Value)> Sent { get; } = [];

        public string? PreviewCommand(string commandId, string? value) => commandId == "WIPE" ? "WIPE\n" : null;

        public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
        {
            Sent.Add((commandId, value));
            return Task.CompletedTask;
        }
    }

    private sealed class Decline : OoBDev.UiDefinitions.ViewModels.IConfirmPrompt
    {
        public string? Asked { get; private set; }

        public bool Confirm(string message)
        {
            Asked = message;
            return false;
        }
    }

    private sealed class FixedColor : OoBDev.UiDefinitions.ViewModels.IColorPicker
    {
        public string? Pick(string? current) => "255,0,0";
    }

    private sealed class FakePanel(FakeSurface surface) : IDevicePanelContribution
    {
        public string Id => "fake";

        public string MenuTitle => "_Fake panel...";

        public string? PresenterName => null;

        public bool IsAvailable(string transport, int vendorId, int productId) => true;

        public UiDefinition BuildDefinition() => new()
        {
            Name = "Fake",
            Sections = [new UiSection { Controls = [new ToggleControl { Id = "d1", Label = "D1" }, new ButtonControl { Id = "wipe", Label = "Wipe", CommandId = "WIPE", ConfirmMessage = "Erase?" }, new TextFieldControl { Id = "color", Label = "Color" }, new ButtonControl { Id = "pick", Label = "Pick", ColorPickerTargetCommandId = "color" }] }],
        };

        public IControlSurface CreateSurface(Session session, IStructuredPresenter? source = null) => surface;
    }

    private sealed class InterlockPanel(FakeSurface surface) : IDevicePanelContribution
    {
        public string Id => "interlock";

        public string MenuTitle => "_Interlock...";

        public string? PresenterName => null;

        public bool IsAvailable(string transport, int vendorId, int productId) => true;

        public UiDefinition BuildDefinition() => new()
        {
            Name = "Interlock",
            Sections = [new UiSection { Controls = [new ToggleControl { Id = "arm", Label = "Arm" }, new ButtonControl { Id = "fire", Label = "Fire", CommandId = "FIRE", EnabledWhen = new UiCondition { Id = "arm", Values = ["True"] }, DisabledReason = "Arm first" }] }],
        };

        public IControlSurface CreateSurface(Session session, IStructuredPresenter? source = null) => surface;
    }

    [TestMethod]
    public async Task OpenDevicePanelCommand_EnabledWhenToggleOn_EnablesTheButton()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton<IDevicePanelContribution>(new InterlockPanel(new FakeSurface()));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.NewLoopbackSession.Execute(null);
        shell.SelectedSession!.Connect.Execute(null);
        await WaitAsync(() => shell.SelectedSession.State == ConnectionState.Open);
        shell.OpenDevicePanelCommand.Execute(shell.DevicePanels[0]);
        var fire = shell.SelectedPanel!.Form.Row("fire")!;
        var disabledBefore = !fire.IsEnabled;

        // Test
        ((OoBDev.UiDefinitions.ViewModels.ToggleViewModel)shell.SelectedPanel.Form.Row("arm")!).Toggle.Execute(null);

        // Assert
        Assert.IsTrue(disabledBefore);
        Assert.IsTrue(fire.IsEnabled);
    }

    [TestMethod]
    public async Task OpenDevicePanelCommand_SurfaceWithPreview_PutsTheSendsTextInTheButtonTooltip()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton<IDevicePanelContribution>(new FakePanel(new FakeSurface()));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.NewLoopbackSession.Execute(null);
        shell.SelectedSession!.Connect.Execute(null);
        await WaitAsync(() => shell.SelectedSession.State == ConnectionState.Open);

        // Test
        shell.OpenDevicePanelCommand.Execute(shell.DevicePanels[0]);

        // Assert
        Assert.AreEqual("Sends: WIPE\n", shell.SelectedPanel!.Form.Row("wipe") is OoBDev.UiDefinitions.ViewModels.ButtonViewModel b ? b.ToolTip : null);
    }

    [TestMethod]
    public async Task OpenDevicePanelCommand_ConnectedSession_ShowsPanelThatSendsToTheSurface()
    {
        // Stage
        var surface = new FakeSurface();
        var services = new ServiceCollection();
        services.AddSingleton<IDevicePanelContribution>(new FakePanel(surface));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.NewLoopbackSession.Execute(null);
        var session = shell.SelectedSession!;
        var panel = shell.DevicePanels[0];
        Assert.IsFalse(shell.OpenDevicePanelCommand.CanExecute(panel));
        session.Connect.Execute(null);
        await WaitAsync(() => session.State == ConnectionState.Open);

        // Test
        shell.OpenDevicePanelCommand.Execute(panel);
        var toggle = (OoBDev.UiDefinitions.ViewModels.ToggleViewModel)shell.SelectedPanel!.Form.Row("d1")!;
        toggle.Toggle.Execute(null);

        // Assert
        Assert.AreEqual("Fake panel", shell.SelectedPanel.Title);
        CollectionAssert.AreEqual(new (string, string?)[] { ("d1", "1") }, surface.Sent);
    }

    [TestMethod]
    public async Task Button_ConfirmDeclined_DoesNotSend()
    {
        // Stage
        var surface = new FakeSurface();
        var decline = new Decline();
        var services = new ServiceCollection();
        services.AddSingleton<IDevicePanelContribution>(new FakePanel(surface));
        services.AddSingleton<OoBDev.UiDefinitions.ViewModels.IConfirmPrompt>(decline);
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.NewLoopbackSession.Execute(null);
        var session = shell.SelectedSession!;
        session.Connect.Execute(null);
        await WaitAsync(() => session.State == ConnectionState.Open);
        shell.OpenDevicePanelCommand.Execute(shell.DevicePanels[0]);
        var wipe = (OoBDev.UiDefinitions.ViewModels.ButtonViewModel)shell.SelectedPanel!.Form.Row("wipe")!;

        // Test
        wipe.Invoke.Execute(null);

        // Assert
        Assert.AreEqual("Erase?", decline.Asked);
        Assert.IsEmpty(surface.Sent);
    }

    [TestMethod]
    public async Task Button_ColorPicked_SendsTheColorToTheTarget()
    {
        // Stage
        var surface = new FakeSurface();
        var services = new ServiceCollection();
        services.AddSingleton<IDevicePanelContribution>(new FakePanel(surface));
        services.AddSingleton<OoBDev.UiDefinitions.ViewModels.IColorPicker>(new FixedColor());
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.NewLoopbackSession.Execute(null);
        var session = shell.SelectedSession!;
        session.Connect.Execute(null);
        await WaitAsync(() => session.State == ConnectionState.Open);
        shell.OpenDevicePanelCommand.Execute(shell.DevicePanels[0]);
        var pick = (OoBDev.UiDefinitions.ViewModels.ButtonViewModel)shell.SelectedPanel!.Form.Row("pick")!;

        // Test
        pick.Invoke.Execute(null);

        // Assert
        CollectionAssert.AreEqual(new (string, string?)[] { ("color", "255,0,0") }, surface.Sent);
    }

    private static async Task WaitAsync(Func<bool> condition)
    {
        var watch = Stopwatch.StartNew();
        while (!condition())
        {
            Assert.IsLessThan(5000, watch.ElapsedMilliseconds, "Timed out waiting for the condition.");
            await Task.Delay(10);
        }
    }
}
