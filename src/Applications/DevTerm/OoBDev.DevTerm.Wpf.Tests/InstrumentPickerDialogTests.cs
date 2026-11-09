using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Loopback;
using OoBDev.System.IO;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The instrument picker and the panel it opens for the selected session.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class InstrumentPickerDialogTests
{
    private sealed class NullSurface : IControlSurface
    {
        public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class StubProvider : IInstrumentPanelProvider
    {
        public bool ThrowOnOpen { get; init; }

        public string? Opened { get; private set; }

        public string Id => "stub";

        public string MenuTitle => "_Stub instrument...";

        public string PickerTitle => "Stub instrument";

        public string PresenterName => "stub-presenter";

        public string AutoDetectChoice => "Auto-detect";

        public string GenericChoice => "Generic";

        public IReadOnlyList<string> ProfileNames => ["Model A", "Model B"];

        public bool IsAvailable(string transport, int vendorId, int productId) => true;

        public bool ConfigureForProfile(string profileName, IPresenter? presenter) => true;

        public InstrumentPanel Open(string choice, Session session, IPresenter? structuredSource)
        {
            if (ThrowOnOpen)
            {
                throw new InvalidOperationException("no instrument");
            }

            Opened = choice;
            var definition = new UiDefinition { Name = "Stub", Sections = [new UiSection { Controls = [new ButtonControl { Id = "go", Label = "Go", CommandId = "GO" }] }] };
            return new InstrumentPanel(definition, new NullSurface(), "Stub: " + choice);
        }

        public string DetectProgressMessage(TimeSpan timeout) => "Detecting...";

        public Task<InstrumentDetection> DetectAsync(Session session, IPresenter? structuredSource, TimeSpan timeout) =>
            Task.FromResult(new InstrumentDetection("Model B", "Found Model B."));
    }

    private sealed class FakePickerDialog(int index) : IInstrumentPickerDialog
    {
        public bool Pick(InstrumentPickerDialogViewModel picker)
        {
            picker.SelectedIndex = index;
            return picker.TryChoose();
        }
    }

    private static async Task<ShellViewModel> OpenShellAsync(StubProvider provider, int index, bool withSession = true)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IInstrumentPickerDialog>(new FakePickerDialog(index));
        services.AddSingleton<IInstrumentPanelProvider>(provider);
        var shell = services.AddDevTermWpfShell().BuildServiceProvider().GetRequiredService<ShellViewModel>();
        if (withSession)
        {
            var transport = new LoopbackTransport(Options.Create(new LoopbackTransportOptions()));
            var session = new Session(transport, new Pipeline([]));
            var tab = new SessionTab(session, new PresenterCatalog([]), new CliOptions());
            var viewModel = new SessionViewModel(session, "Loopback", tab);
            shell.Sessions.Add(viewModel);
            shell.SelectedSession = viewModel;
            viewModel.Connect.Execute(null);
            for (var i = 0; i < 200 && shell.SelectedSession.State != ConnectionState.Open; i++)
            {
                await Task.Delay(25);
            }
        }

        return shell;
    }

    [TestMethod]
    public void Constructor_Provider_ListsAutoDetectGenericAndProfiles()
    {
        // Stage
        var picker = new InstrumentPickerDialogViewModel(new StubProvider());

        // Assert
        CollectionAssert.AreEqual(new[] { "Auto-detect", "Generic", "Model A", "Model B" }, new List<string>(picker.Choices));
        Assert.AreEqual("Stub instrument", picker.Title);
    }

    [TestMethod]
    public void TryChoose_NothingSelected_ReturnsFalse()
    {
        // Stage
        var picker = new InstrumentPickerDialogViewModel(new StubProvider()) { SelectedIndex = -1 };

        // Test
        var chosen = picker.TryChoose();

        // Assert
        Assert.IsFalse(chosen);
        Assert.IsNull(picker.Chosen);
    }

    [TestMethod]
    public async Task OpenInstrumentPanel_NoSession_IsDisabled()
    {
        // Stage
        var provider = new StubProvider();
        var shell = await OpenShellAsync(provider, 2, withSession: false);

        // Assert
        Assert.IsFalse(shell.OpenInstrumentPanel.CanExecute(provider));
    }

    [TestMethod]
    public async Task OpenInstrumentPanel_ChosenProfile_ShowsThePanel()
    {
        // Stage
        var provider = new StubProvider();
        var shell = await OpenShellAsync(provider, 2);

        // Test
        shell.OpenInstrumentPanel.Execute(provider);
        await Task.Delay(100);

        // Assert
        Assert.AreEqual("Model A", provider.Opened);
        Assert.AreEqual("Stub: Model A", shell.SelectedPanel!.Title);
    }

    [TestMethod]
    public async Task OpenInstrumentPanel_AutoDetect_WritesProgressAndFoundMessage()
    {
        // Stage
        var provider = new StubProvider();
        var shell = await OpenShellAsync(provider, 0);

        // Test
        shell.OpenInstrumentPanel.Execute(provider);
        await Task.Delay(100);

        // Assert
        Assert.AreEqual("Model B", provider.Opened);
        CollectionAssert.IsSubsetOf(new[] { "Detecting...", "Found Model B." }, new List<string>(shell.SelectedSession!.OutputLines));
    }

    [TestMethod]
    public async Task OpenInstrumentPanel_ProviderThrows_ReportsInTheOutput()
    {
        // Stage
        var provider = new StubProvider { ThrowOnOpen = true };
        var shell = await OpenShellAsync(provider, 1);

        // Test
        shell.OpenInstrumentPanel.Execute(provider);
        await Task.Delay(100);

        // Assert
        CollectionAssert.Contains(new List<string>(shell.SelectedSession!.OutputLines), "Could not open the instrument panel: no instrument");
    }
}
