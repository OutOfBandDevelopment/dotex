using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Panels;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.Velleman.K8055.Panels;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>Device-menu entries for contributed panels: listed under Device, enabled only for a matching connection.</summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class TuiPluginPanelsTests
{
    public required TestContext TestContext { get; set; }

    private static MenuItem? FindItem(View window)
    {
        var bar = window.SubViews.OfType<MenuBar>().Single();
        var device = bar.SubViews.OfType<MenuBarItem>().Single(i => i.Title.Replace("_", string.Empty, StringComparison.Ordinal) == "Device");
        return device.PopoverMenu!.SubViews.OfType<Menu>().SelectMany(m => m.SubViews.OfType<MenuItem>()).FirstOrDefault(i => i.Title == "_K8055 Control Panel...");
    }

    [TestMethod]
    public async Task ContributedPanel_AppearsInTheDeviceMenu_EnabledOnlyForAMatchingConnection()
    {
        foreach (var (options, expected) in new[]
        {
            (new CliOptions { Transport = "hid", VendorId = 0x10CF, ProductId = 0x5500, Presenter = ["k8055"] }, true),
            (new CliOptions { Transport = "tcp", Host = "127.0.0.1", Port = "23" }, false),
        })
        {
            var presenter = new AsciiPresenter(Options.Create(new AsciiPresenterOptions()));
            options.Parser ??= presenter.Name;
            var session = new Session(new FakeTransport(), new Pipeline([presenter]));
            await session.OpenAsync(TestContext.CancellationToken);

            TuiTestRunner.RunHeadlessApp(app =>
            {
                var parts = TuiMode.BuildWindow(app, session, new PresenterCatalog([presenter]), options, TuiTestRunner.EmptyProfiles(), panels: [new DevicePanelContribution(new K8055Panel())]);
                var item = FindItem(parts.Window);

                Assert.IsNotNull(item, "The contributed panel should be listed under Device.");
                Assert.AreEqual(expected, item.Enabled);
            });

            await session.CloseAsync(TestContext.CancellationToken);
        }
    }
}
