using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The instrument picker window listing auto-detect, the generic panel and two profiles, captured for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class InstrumentPickerScreenshotTests
{
    internal sealed class GuideProvider : IInstrumentPanelProvider
    {
        public string Id => "scpi";

        public string MenuTitle => "_SCPI Instrument...";

        public string PickerTitle => "SCPI Instrument";

        public string PresenterName => "scpi";

        public string AutoDetectChoice => "Auto-detect (*IDN?)";

        public string GenericChoice => "Generic SCPI instrument";

        public IReadOnlyList<string> ProfileNames => ["Keysight 34401A multimeter", "Rigol DP832 power supply"];

        public bool IsAvailable(string transport, int vendorId, int productId) => true;

        public bool ConfigureForProfile(string profileName, IPresenter? presenter) => true;

        public InstrumentPanel Open(string choice, Session session, IPresenter? structuredSource) => throw new NotSupportedException();

        public string DetectProgressMessage(TimeSpan timeout) => string.Empty;

        public Task<InstrumentDetection> DetectAsync(Session session, IPresenter? structuredSource, TimeSpan timeout) => throw new NotSupportedException();
    }

    [TestMethod]
    public void InstrumentPickerWindow_ScpiChoices_IsCaptured() =>
        WpfScreenshot.Capture(
            () => new InstrumentPickerWindow(new InstrumentPickerDialogViewModel(new GuideProvider())),
            "wpf-instrument-picker.png",
            460,
            340);
}
