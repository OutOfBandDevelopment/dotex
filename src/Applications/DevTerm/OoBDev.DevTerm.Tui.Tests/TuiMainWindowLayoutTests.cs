using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OoBDev.System.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.DevTerm;
using Microsoft.Extensions.Options;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// Layout regression tests (see <see cref="TuiLayoutAssert"/>) for the TUI main window
/// (<see cref="TuiMode"/>) at the minimum, a common and a large terminal size, in both themes -
/// connected, disconnected with a startup error, with a long reply, logging, and with each menu open
/// (both connected and, for File/Device, disconnected). Each also saves a review PNG under
/// <c>artifacts/ui-review/tui/</c> (see <see cref="TuiReview"/>).
/// </summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class TuiMainWindowLayoutTests
{
    [TestCleanup]
    public void Cleanup() => TuiReview.ResetTheme();

    private static (Session Session, PresenterCatalog Catalog, CliOptions Options) Create()
    {
        var ascii = new AsciiPresenter(Options.Create(new AsciiPresenterOptions()));
        var hex = new HexPresenter();
        var session = new Session(new FakeTransport(), new Pipeline([ascii]));
        var options = new CliOptions { Transport = "tcp", Host = "192.168.0.107", Port = "23", Presenter = ["ascii"], Parser = "ascii" };
        return (session, new PresenterCatalog([ascii, hex]), options);
    }

    private static TuiWindowParts? _parts;

    private static Func<IApplication, View> MainWindow(Session session, PresenterCatalog catalog, CliOptions options, string? message = null) => app =>
    {
        _parts = TuiMode.BuildWindow(app, session, catalog, options, TuiTestRunner.EmptyProfiles(), message, panels: TuiTestRunner.BuiltInPanels, instruments: TuiTestRunner.BuiltInInstruments);
        _parts.SendField.SetFocus();
        return _parts.Window;
    };

    public static IEnumerable<object[]> SizesAndThemes =>
        from theme in new[] { "light", "dark" }
        from size in TuiReview.Sizes
        select new object[] { size.Width, size.Height, theme };

    public static IEnumerable<object[]> Sizes => TuiReview.Sizes.Select(s => new object[] { s.Width, s.Height });

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public async Task Connected(int width, int height, string theme)
    {
        var (session, catalog, options) = Create();
        await session.OpenAsync(TestContext.CancellationToken);
        TuiReview.Screen("main-connected", width, height, theme, MainWindow(session, catalog, options));
        await session.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    [DynamicData(nameof(SizesAndThemes))]
    public void DisconnectedWithAStartupError(int width, int height, string theme)
    {
        var (session, catalog, options) = Create();
        var error = $"{ConnectionErrorMessages.For("tcp", new global::System.Net.Sockets.SocketException(10061))} Use File > Connect to retry, or File > Device Profiles... to choose another connection.";
        TuiReview.Screen("main-disconnected", width, height, theme, MainWindow(session, catalog, options, error));
    }

    [TestMethod]
    [DynamicData(nameof(Sizes))]
    public async Task WithALongReply(int width, int height)
    {
        var (session, catalog, options) = Create();
        await session.OpenAsync(TestContext.CancellationToken);
        TuiReview.Screen("main-long-reply", width, height, "light", MainWindow(session, catalog, options), (app, _) =>
        {
            var lines = Enumerable.Range(1, 80).Select(i => $"[ascii] reading {i}: " + string.Join(",", Enumerable.Range(0, 30).Select(n => (n * i % 97).ToString("00", global::System.Globalization.CultureInfo.InvariantCulture))));
            _parts!.Output.Text = string.Join('\n', lines);
            _parts.Output.CaretOffset = _parts.Output.Text.Length;
        });
        await session.CloseAsync(TestContext.CancellationToken);
    }

    [TestMethod]
    [DynamicData(nameof(Sizes))]
    public async Task Logging(int width, int height)
    {
        var (session, catalog, options) = Create();
        await session.OpenAsync(TestContext.CancellationToken);
        using var folder = new TempDirectoryHandle();
        var directory = folder.DirectoryPath;
        try
        {
            TuiReview.Screen("main-logging", width, height, "light", MainWindow(session, catalog, options), (app, _) =>
            {
                Assert.IsTrue(_parts!.Logging.Start(Path.Combine(directory, "20260925-120000_tcp_192.168.0.107_23.jsonl")));
            });
        }
        finally
        {
            _parts?.Logging.Logger()?.Dispose();
            await session.CloseAsync(TestContext.CancellationToken);
        }
    }

    [TestMethod]
    [DataRow("File")]
    [DataRow("Send as")]
    [DataRow("Device")]
    [DataRow("View")]
    public async Task WithAMenuOpen(string menu)
    {
        foreach (var (width, height) in TuiReview.Sizes)
        {
            var (session, catalog, options) = Create();
            await session.OpenAsync(TestContext.CancellationToken);
            TuiReview.Screen($"main-menu-{menu.Replace(' ', '-').ToLowerInvariant()}", width, height, "light", MainWindow(session, catalog, options), (app, window) => OpenMenu(window, menu), OverMenu);
            await session.CloseAsync(TestContext.CancellationToken);
        }
    }

    /// <summary>The main window's menus while disconnected (a startup connect failure), never reviewed
    /// alongside <see cref="WithAMenuOpen"/>'s connected captures.</summary>
    [TestMethod]
    [DataRow("File")]
    [DataRow("Device")]
    public void WithAMenuOpen_Disconnected(string menu)
    {
        foreach (var (width, height) in TuiReview.Sizes)
        {
            var (session, catalog, options) = Create();
            var error = $"{ConnectionErrorMessages.For("tcp", new global::System.Net.Sockets.SocketException(10061))} Use File > Connect to retry, or File > Device Profiles... to choose another connection.";
            TuiReview.Screen($"main-menu-{menu.ToLowerInvariant()}-disconnected", width, height, "light", MainWindow(session, catalog, options, error), (app, window) => OpenMenu(window, menu), OverMenu);
        }
    }

    [TestMethod]
    public async Task WithTheThemeSubmenuOpen()
    {
        foreach (var (width, height) in TuiReview.Sizes)
        {
            var (session, catalog, options) = Create();
            await session.OpenAsync(TestContext.CancellationToken);
            TuiReview.Screen("main-menu-view-theme", width, height, "dark", MainWindow(session, catalog, options), (app, window) =>
            {
                var popover = OpenMenu(window, "View");
                // Exact match, not Contains("Theme") - "_Build/Edit Theme..." is a sibling leaf item
                // that would also match a substring search.
                var themeItem = popover.Root!.SubViews.OfType<MenuItem>().Single(i => i.Title == "_Theme");
                // Not public in v2.5.0: what hovering or arrowing onto the item does.
                typeof(PopoverMenu).GetMethod("ShowMenuItemSubMenu", global::System.Reflection.BindingFlags.Instance | global::System.Reflection.BindingFlags.NonPublic | global::System.Reflection.BindingFlags.Public)!.Invoke(popover, [themeItem]);
            }, OverMenu);
            await session.CloseAsync(TestContext.CancellationToken);
        }
    }

    /// <summary>Opens a top-level menu the way clicking it does; returns its popover.</summary>
    internal static PopoverMenu OpenMenu(View window, string title)
    {
        var bar = window.SubViews.OfType<MenuBar>().Single();
        var item = bar.SubViews.OfType<MenuBarItem>().Single(i => i.Title.Replace("_", string.Empty, StringComparison.Ordinal) == title);
        item.PopoverMenuOpen = true;
        return item.PopoverMenu!;
    }

    /// <summary>A menu is drawn over the window, so the window's own on-screen text check is off; every open menu (a popover, outside the window's tree) is checked on its own instead.</summary>
    internal static TuiLayoutOptions OverMenu(IApplication app, View window)
    {
        foreach (var menu in app.Popovers!.Popovers.OfType<PopoverMenu>().Where(p => p.Visible))
        {
            foreach (var open in menu.SubViews.OfType<Menu>().Where(m => m.Visible))
            {
                var problems = TuiLayoutAssert.FindProblems(app, open, new TuiLayoutOptions { CheckRenderedText = false });
                Assert.IsEmpty(problems, string.Join("\n", problems));
            }
        }

        return new TuiLayoutOptions { CheckRenderedText = false };
    }

    public required TestContext TestContext { get; set; }
}
