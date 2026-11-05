using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Devices.Scpi;
using OoBDev.DevTerm.Panels;
using OoBDev.Kuando.Busylight.Panels;
using OoBDev.Scpi;
using OoBDev.UiDefinitions;
using OoBDev.Velleman.K8055.Panels;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;

namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// Layout regression tests (see <see cref="TuiLayoutAssert"/>) for the TUI control panels
/// (<see cref="ControlPanelMode"/>): the K8055, the Busylight (with and without a custom color) and every
/// bundled SCPI profile plus Generic, each scrolled to the top and to the bottom, at each review size, a few
/// in the dark theme too. Each also saves a review PNG under <c>artifacts/ui-review/tui/</c>.
/// </summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class TuiPanelLayoutTests
{
    private static ControlPanelWindowParts? _parts;

    public required TestContext TestContext { get; set; }

    public static IEnumerable<object[]> Sizes => TuiReview.Sizes.Select(s => new object[] { s.Width, s.Height });

    public static IEnumerable<object[]> SizesAndThemes =>
        from theme in new[] { "light", "dark" }
        from size in TuiReview.Sizes
        select new object[] { size.Width, size.Height, theme };

    public static IEnumerable<ScpiInstrumentProfile> Profiles => [ScpiProfileCatalog.Generic, .. ScpiProfileCatalog.Create().Profiles];

    public static IEnumerable<object[]> ProfilesSizesAndThemes =>
        from profile in Profiles
        from theme in new[] { "light", "dark" }
        from size in TuiReview.Sizes
        select new object[] { profile.Name, size.Width, size.Height, theme };

    [TestCleanup]
    public void Cleanup()
    {
        TuiReview.ResetTheme();
        LastPickedColors.Forget("customColor");
    }

    private static Func<IApplication, View> Panel(UiDefinition definition, IControlSurface surface, IPresenter? structured, string title) => app =>
    {
        SectionExpansionState.Forget(definition.Name);
        _parts = ControlPanelMode.BuildWindow(app, definition, surface, structured, title);
        return _parts.Window;
    };

    private static void ScrollToBottom(IApplication app, View window)
    {
        var form = _parts!.FormContent;
        form.Viewport = form.Viewport with { Y = Math.Max(0, form.GetContentSize().Height - form.Viewport.Height) };
    }

    private static Session NewSession() => new(new FakeTransport(), new Pipeline([]));

    private static DeviceSend SendThrough(Session session) => (data, cancellationToken) => session.SendAsync(data, cancellationToken);

    private static string Slug(string name) => new([.. name.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : '-')]);

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void K8055(int width, int height, string theme)
    {
        var session = NewSession();
        var build = Panel(K8055UiDefinition.Build(), new K8055ControlSurface(SendThrough(session)), new DeviceDecoderPresenter(new K8055Decoder()), "dev-term — K8055 Control Panel");
        TuiReview.Screen("panel-k8055-top", width, height, theme, build);
        TuiReview.Screen("panel-k8055-bottom", width, height, theme, build, ScrollToBottom);
    }

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void K8055_WithLiveData(int width, int height, string theme)
    {
        var transport = new FakeTransport();
        var decoder = new DeviceDecoderPresenter(new K8055Decoder());
        var session = new Session(transport, new Pipeline([decoder]));
        session.OpenAsync(TestContext.CancellationToken).GetAwaiter().GetResult();
        var build = Panel(K8055UiDefinition.Build(), new K8055ControlSurface(SendThrough(session)), decoder, "dev-term — K8055 Control Panel");

        TuiReview.UseTheme(theme);
        string? failure = null;
        try
        {
            TuiTestRunner.RunWithLoopApp(
                app => app.Driver!.SetScreenSize(width, height),
                build,
                (app, root) =>
                {
                    // [reportId, digitalInRaw, 0x03, analogIn1, analogIn2, counter1Lo, counter1Hi, counter2Lo, counter2Hi]
                    transport.PushIncomingAsync([0x00, 0x05, 0x03, 42, 80, 5, 0, 10, 0]).GetAwaiter().GetResult();
                    Assert.IsTrue(TuiTestRunner.WaitUntilOnLoop(() => _parts!.IndicatorLabels["analogIn1"].Text == "42", TimeSpan.FromSeconds(5)), "Expected the Analog In 1 indicator to reflect the decoded frame.");
                    failure = TuiTestRunner.InvokeOnLoop(() =>
                    {
                        app.LayoutAndDraw(true);
                        return TuiReview.Check(app, root, "panel-k8055-live", theme);
                    });
                });
        }
        finally
        {
            session.CloseAsync(TestContext.CancellationToken).GetAwaiter().GetResult();
            TuiReview.ResetTheme();
        }

        Assert.IsNull(failure, failure);
    }

    [TestMethod]
    [DynamicData(nameof(Sizes))]
    public void Busylight(int width, int height)
    {
        LastPickedColors.Forget("customColor");
        var build = Panel(BusylightUiDefinition.Build(), new BusylightControlSurface(SendThrough(NewSession())), null, "dev-term — Busylight Control Panel");
        TuiReview.Screen("panel-busylight", width, height, "light", build);
    }

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void Busylight_WithACustomColor(int width, int height, string theme)
    {
        LastPickedColors.Set("customColor", (0xFF, 0x66, 0x00));
        var build = Panel(BusylightUiDefinition.Build(), new BusylightControlSurface(SendThrough(NewSession())), null, "dev-term — Busylight Control Panel");
        TuiReview.Screen("panel-busylight-custom", width, height, theme, build);
    }

    [TestMethod]
    [DynamicData(nameof(ProfilesSizesAndThemes))]
    public void EachScpiProfile(string profileName, int width, int height, string theme)
    {
        var profile = Profiles.Single(p => p.Name == profileName);
        var presenter = new ScpiReplyPresenter();
        var build = Panel(ScpiUiDefinitionBuilder.Build(profile), new ScpiControlSurface(NewSession(), profile, presenter), presenter, $"dev-term — {profile.Name}");
        TuiReview.Screen($"panel-scpi-{Slug(profile.Name)}-top", width, height, theme, build);
        TuiReview.Screen($"panel-scpi-{Slug(profile.Name)}-bottom", width, height, theme, build, ScrollToBottom);
    }

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void ScpiPanel_WithAFocusedControlsPreview(int width, int height, string theme)
    {
        var profile = ScpiProfileCatalog.Create().Profiles.Single(p => p.Name.Contains("34401A", StringComparison.OrdinalIgnoreCase));
        var build = Panel(ScpiUiDefinitionBuilder.Build(profile), new ScpiControlSurface(NewSession(), profile, tracker: null), null, $"dev-term — {profile.Name}");
        TuiReview.Screen("panel-scpi-preview", width, height, theme, build, (app, _) => _parts!.ControlViews["confFres.send"].SetFocus());
    }
}
